-- Тестовый каталог агротоваров для прогона catalog-search-run.md.
-- Образец ассортимента: agro-him.com.ua. Цены и фасовки ориентировочные.
BEGIN;

CREATE FUNCTION pg_temp.slugify(s text) RETURNS text LANGUAGE sql IMMUTABLE AS $$
  SELECT trim(both '-' from regexp_replace(
    replace(replace(replace(replace(replace(replace(replace(replace(replace(
      translate(lower(s), 'абвгґдезиіїйклмнопрстуфь''’', 'abvhgdezyiiiklmnoprstuf'),
      'ж','zh'),'ч','ch'),'ш','sh'),'щ','shch'),'ю','iu'),'я','ia'),'є','ie'),
      'х','kh'),'ц','ts'),
    '[^a-z0-9]+', '-', 'g'));
$$;

-- Страны
INSERT INTO countries (id, name, slug) VALUES
  (uuidv7(), 'Україна', 'ukraina'),
  (uuidv7(), 'Швейцарія', 'shveitsariia'),
  (uuidv7(), 'Німеччина', 'nimechchyna'),
  (uuidv7(), 'Норвегія', 'norvehiia'),
  (uuidv7(), 'Ізраїль', 'izrail');

-- Производители: смесь кириллических и латинских имён — нужна для шага 3.
INSERT INTO manufacturers (id, name, slug, country_id)
SELECT uuidv7(), m.name, m.slug, (SELECT id FROM countries WHERE slug = m.country)
FROM (VALUES
  ('Укравіт', 'ukravit', 'ukraina'),
  ('Агромакси', 'ahromaksy', 'ukraina'),
  ('Сімейний Сад', 'simeinyi-sad', 'ukraina'),
  ('Ензим Агро', 'enzym-ahro', 'ukraina'),
  ('Syngenta', 'syngenta', 'shveitsariia'),
  ('Bayer', 'bayer', 'nimechchyna'),
  ('BASF', 'basf', 'nimechchyna'),
  ('Nordox', 'nordox', 'norvehiia'),
  ('ADAMA', 'adama', 'izrail')
) AS m(name, slug, country);

-- Категории: корень с тремя детьми (листинг корня идёт по поддереву) и отдельный корень.
INSERT INTO categories (id, name, slug, parent_id, display_order)
VALUES (uuidv7(), 'Засоби захисту рослин', 'zasoby-zakhystu-roslyn', NULL, 0),
       (uuidv7(), 'Родентициди', 'rodentytsydy', NULL, 1);

INSERT INTO categories (id, name, slug, parent_id, display_order)
SELECT uuidv7(), c.name, c.slug, (SELECT id FROM categories WHERE slug = 'zasoby-zakhystu-roslyn'), c.ord
FROM (VALUES ('Гербіциди', 'herbitsydy', 0),
             ('Фунгіциди', 'funhitsydy', 1),
             ('Інсектициди', 'insektytsydy', 2)) AS c(name, slug, ord);

-- Атрибуты
INSERT INTO product_attributes (id, name, slug) VALUES
  (uuidv7(), 'Діюча речовина', 'diiucha-rechovyna'),
  (uuidv7(), 'Препаративна форма', 'preparatyvna-forma');

INSERT INTO attribute_values (id, attribute_id, name, slug)
SELECT uuidv7(), (SELECT id FROM product_attributes WHERE slug = 'diiucha-rechovyna'), v, pg_temp.slugify(v)
FROM unnest(ARRAY['гліфосат','дикамба','метрибузин','хізалофоп-П-етил','дифеноконазол','манкоцеб',
                  'металаксил','мідь','тебуконазол','ципродиніл','сірка','піраклостробін',
                  'тіаметоксам','імідаклоприд','дельтаметрин','лямбда-цигалотрин',
                  'альфа-циперметрин','циперметрин','бродіфакум']) AS v;

INSERT INTO attribute_values (id, attribute_id, name, slug)
SELECT uuidv7(), (SELECT id FROM product_attributes WHERE slug = 'preparatyvna-forma'), v, pg_temp.slugify(v)
FROM unnest(ARRAY['ВР','КС','КЕ','ЗП','ВГ','МЕ','Гранули','Тісто']) AS v;

INSERT INTO category_attributes (category_id, attribute_id, display_order)
SELECT c.id, a.id, CASE a.slug WHEN 'diiucha-rechovyna' THEN 0 ELSE 1 END
FROM categories c CROSS JOIN product_attributes a;

-- Товары: имя | производитель | категория | форма | действующие вещества | фасовки (значение:единица:цена;...)
CREATE TEMP TABLE seed (name text, mfr text, cat text, form text, actives text[], variants text) ON COMMIT DROP;
INSERT INTO seed VALUES
  -- Гербіциди
  ('Гліфовіт Екстра', 'ukravit', 'herbitsydy', 'ВР', '{гліфосат}', '1:Liter:494.52;100:Milliliter:62.00'),
  ('Антибур''ян', 'ukravit', 'herbitsydy', 'ВР', '{гліфосат}', '500:Milliliter:270.74'),
  ('Антипирій', 'ukravit', 'herbitsydy', 'КЕ', '{хізалофоп-П-етил}', '100:Milliliter:87.42'),
  ('Тройсет', 'ukravit', 'herbitsydy', 'ВГ', '{метрибузин}', '25:Gram:27.47'),
  ('Ураган Форте', 'syngenta', 'herbitsydy', 'ВР', '{гліфосат}', '300:Milliliter:269.21;1:Liter:690.00'),
  ('Напалм', 'simeinyi-sad', 'herbitsydy', 'ВР', '{гліфосат}', '100:Milliliter:75.03'),
  ('Дикамба Форте', 'simeinyi-sad', 'herbitsydy', 'ВР', '{дикамба}', '15:Milliliter:16.94'),
  ('Буран', 'ahromaksy', 'herbitsydy', 'ВР', '{гліфосат}', '100:Milliliter:37.99'),
  ('Раундап', 'bayer', 'herbitsydy', 'ВР', '{гліфосат}', '1:Liter:520.00'),
  ('Зенкор', 'bayer', 'herbitsydy', 'ВГ', '{метрибузин}', '20:Gram:45.00'),
  ('Чисте поле', 'adama', 'herbitsydy', 'ВР', '{гліфосат}', '300:Milliliter:132.06;500:Milliliter:254.80'),
  -- Фунгіциди
  ('Скор', 'syngenta', 'funhitsydy', 'КЕ', '{дифеноконазол}', '2:Milliliter:21.67;50:Milliliter:290.00'),
  ('Ридоміл Голд R', 'syngenta', 'funhitsydy', 'ЗП', '{манкоцеб,металаксил}', '100:Gram:101.11'),
  ('Хорус', 'syngenta', 'funhitsydy', 'ВГ', '{ципродиніл}', '3:Gram:38.00'),
  ('Тіовіт Джет', 'syngenta', 'funhitsydy', 'ВГ', '{сірка}', '30:Gram:22.00'),
  ('Нордокс', 'nordox', 'funhitsydy', 'ЗП', '{мідь}', '10:Gram:24.36;50:Gram:95.00'),
  ('Пенкоцеб', 'simeinyi-sad', 'funhitsydy', 'ЗП', '{манкоцеб}', '20:Gram:15.74'),
  ('Сальто', 'simeinyi-sad', 'funhitsydy', 'КС', '{тебуконазол}', '100:Milliliter:87.13'),
  ('Медян Екстра', 'simeinyi-sad', 'funhitsydy', 'КС', '{мідь}', '20:Milliliter:24.93'),
  ('Сільвер', 'ukravit', 'funhitsydy', 'КС', '{тебуконазол}', '30:Milliliter:23.48'),
  ('Бордоська суміш', 'ahromaksy', 'funhitsydy', 'ЗП', '{мідь}', '300:Gram:50.62'),
  ('Мідний купорос', 'ahromaksy', 'funhitsydy', 'ЗП', '{мідь}', '100:Gram:36.50'),
  ('Фалькон', 'bayer', 'funhitsydy', 'КЕ', '{тебуконазол}', '10:Milliliter:42.00'),
  ('Кабріо Топ', 'basf', 'funhitsydy', 'ВГ', '{піраклостробін}', '10:Gram:58.00'),
  -- Інсектициди
  ('Актара', 'syngenta', 'insektytsydy', 'ВГ', '{тіаметоксам}', '1.4:Gram:22.00;4:Gram:55.00'),
  ('Карате Зеон', 'syngenta', 'insektytsydy', 'КС', '{лямбда-цигалотрин}', '4:Milliliter:30.00'),
  ('Конфідор', 'bayer', 'insektytsydy', 'КС', '{імідаклоприд}', '1:Milliliter:18.00'),
  ('Децис Профі', 'bayer', 'insektytsydy', 'ВГ', '{дельтаметрин}', '1:Gram:21.00'),
  ('Фастак', 'basf', 'insektytsydy', 'КЕ', '{альфа-циперметрин}', '2:Milliliter:19.00'),
  ('Антихрущ', 'ukravit', 'insektytsydy', 'КС', '{імідаклоприд}', '150:Milliliter:177.33'),
  ('Джек Пот', 'ukravit', 'insektytsydy', 'КЕ', '{лямбда-цигалотрин}', '4:Milliliter:13.48'),
  ('Шарпей', 'ukravit', 'insektytsydy', 'МЕ', '{циперметрин}', '1.5:Milliliter:12.00'),
  ('Ентоцид', 'enzym-ahro', 'insektytsydy', 'ЗП', '{}', '100:Gram:81.60;400:Gram:259.28'),
  -- Родентициди
  ('Смерть гризунам', 'ahromaksy', 'rodentytsydy', 'Гранули', '{бродіфакум}', '200:Gram:22.77;600:Gram:22.77'),
  ('Котофеїч', 'ahromaksy', 'rodentytsydy', 'Тісто', '{бродіфакум}', '300:Gram:46.72'),
  ('ПацюкOFF', 'ukravit', 'rodentytsydy', 'Тісто', '{бродіфакум}', '200:Gram:21.36'),
  ('Захисник', 'ukravit', 'rodentytsydy', 'Тісто', '{бродіфакум}', '300:Gram:84.60');

-- По одной вставке на строку, чтобы uuidv7 шли в порядке списка («новизна» предсказуема).
DO $$
DECLARE r record; pid uuid; v text; parts text[]; seq int := 0; unit_sym text;
BEGIN
  FOR r IN SELECT * FROM seed LOOP
    pid := uuidv7();
    INSERT INTO products (id, name, description, category_id, manufacturer_id)
    VALUES (pid, r.name, NULL,
            (SELECT id FROM categories WHERE slug = r.cat),
            (SELECT id FROM manufacturers WHERE slug = r.mfr));

    INSERT INTO product_attribute_values (product_id, attribute_value_id)
    SELECT pid, av.id FROM attribute_values av JOIN product_attributes a ON a.id = av.attribute_id
    WHERE (a.slug = 'preparatyvna-forma' AND av.name = r.form)
       OR (a.slug = 'diiucha-rechovyna' AND av.name = ANY (r.actives));

    FOREACH v IN ARRAY string_to_array(r.variants, ';') LOOP
      parts := string_to_array(v, ':');
      seq := seq + 1;
      unit_sym := CASE parts[2] WHEN 'Liter' THEN 'l' WHEN 'Milliliter' THEN 'ml'
                                WHEN 'Gram' THEN 'g' WHEN 'Kilogram' THEN 'kg' ELSE 'pcs' END;
      INSERT INTO product_variants (id, product_id, sku, slug, price, stock_quantity,
                                    packaging_unit, packaging_value, packaging_key)
      VALUES (uuidv7(), pid, 'AGRO-' || lpad(seq::text, 3, '0'),
              pg_temp.slugify(r.name) || '-' || replace(parts[1], '.', '-') || unit_sym,
              parts[3]::numeric, 50, parts[2], parts[1]::numeric,
              parts[1] || '-' || lower(parts[2]));
    END LOOP;
  END LOOP;
END $$;

COMMIT;

SELECT (SELECT count(*) FROM products) products, (SELECT count(*) FROM product_variants) variants,
       (SELECT count(*) FROM manufacturers) manufacturers, (SELECT count(*) FROM product_attribute_values) pav;
