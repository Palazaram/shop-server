-- Добор к seed-agro.sql: товары со спецсимволами LIKE в названии, отдельная корневая категория.
BEGIN;
INSERT INTO categories (id, name, slug, parent_id, display_order)
VALUES (uuidv7(), 'Добрива та стимулятори', 'dobryva-ta-stymuliatory', NULL, 2);
INSERT INTO category_attributes (category_id, attribute_id, display_order)
SELECT c.id, a.id, CASE a.slug WHEN 'diiucha-rechovyna' THEN 0 ELSE 1 END
FROM categories c CROSS JOIN product_attributes a WHERE c.slug = 'dobryva-ta-stymuliatory';
WITH p AS (
  INSERT INTO products (id, name, description, category_id, manufacturer_id)
  SELECT uuidv7(), v.name, NULL, (SELECT id FROM categories WHERE slug = 'dobryva-ta-stymuliatory'),
         (SELECT id FROM manufacturers WHERE slug = v.mfr)
  FROM (VALUES ('Гумат калію 20%', 'ukravit', 'humat-kaliiu-20-500ml', 'AGRO-101', 58.40),
               ('Гумат калію 200', 'ahromaksy', 'humat-kaliiu-200-500ml', 'AGRO-102', 61.90),
               ('Біомаг 20x', 'simeinyi-sad', 'biomah-20x-250ml', 'AGRO-103', 44.10)) AS v(name, mfr, slug, sku, price)
  RETURNING id, name)
INSERT INTO product_variants (id, product_id, sku, slug, price, stock_quantity, packaging_unit, packaging_value, packaging_key)
SELECT uuidv7(), p.id, v.sku, v.slug, v.price, 50, 'Milliliter', v.qty, v.qty || '-milliliter'
FROM p JOIN (VALUES ('Гумат калію 20%', 'humat-kaliiu-20-500ml', 'AGRO-101', 58.40, 500),
                    ('Гумат калію 200', 'humat-kaliiu-200-500ml', 'AGRO-102', 61.90, 500),
                    ('Біомаг 20x', 'biomah-20x-250ml', 'AGRO-103', 44.10, 250)) AS v(name, slug, sku, price, qty)
  ON v.name = p.name;
COMMIT;
SELECT c.id, p.name FROM products p JOIN categories c ON c.id = p.category_id WHERE c.slug = 'dobryva-ta-stymuliatory';
