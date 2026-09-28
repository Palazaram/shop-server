BEGIN;
INSERT INTO categories (id, name, slug, parent_id, display_order)
SELECT uuidv7(), 'Синт ' || g, 'synt-' || g, NULL, 100 + g FROM generate_series(1, 40) g;
-- 50 000 товаров: 48 000 в синтетических категориях, 2 000 в гербицидах (поддерево корня)
INSERT INTO products (id, name, description, category_id, manufacturer_id)
SELECT uuidv7(),
       (ARRAY['Гербостоп','Фунгомакс','Інсектор','Агрозахист','Ростдобр','Садовик','Полезахист','Мікродобр'])[1 + g % 8]
         || ' ' || (ARRAY['Екстра','Форте','Про','Ультра','Макс','Преміум'])[1 + (g / 8) % 6] || ' ' || g,
       NULL,
       CASE WHEN g <= 50000 THEN '01a0e6d1-5272-7ac8-9787-0ff9786ed6f1'::uuid
            ELSE (SELECT id FROM categories WHERE slug = 'synt-' || (1 + g % 40)) END,
       (SELECT id FROM manufacturers ORDER BY id OFFSET (g % 9) LIMIT 1)
FROM generate_series(1, 50000) g;
INSERT INTO product_variants (id, product_id, sku, slug, price, stock_quantity, packaging_unit, packaging_value, packaging_key)
SELECT uuidv7(), p.id, 'SYN-' || row_number() OVER (), 'syn-' || row_number() OVER (), 10 + random() * 500, 10, 'Milliliter', 100, '100-milliliter' FROM products p WHERE p.name ~ ' [0-9]+$';
ANALYZE product_variants; ANALYZE products; ANALYZE manufacturers; ANALYZE categories;
SELECT (SELECT count(*) FROM products) p, (SELECT count(*) FROM product_variants) v;
EXPLAIN (ANALYZE, COSTS OFF, BUFFERS OFF)
SELECT count(*)::int
FROM product_variants AS p
INNER JOIN products AS p1 ON p.product_id = p1.id
INNER JOIN manufacturers AS m ON p1.manufacturer_id = m.id
WHERE p.product_id IN (
    SELECT p0.id
    FROM products AS p0
    WHERE p0.category_id = ANY (ARRAY['01a0e6d1-5271-7537-9381-09052812bae8','01a0e6d1-5272-7ac8-9787-0ff9786ed6f1','01a0e6d1-5272-7be6-b093-5e0d24d629ae','01a0e6d1-5272-7c07-8f79-0f70777380ba']::uuid[]) AND (p0.name ILIKE '%Ридомил%' ESCAPE '\' OR ('Ридомил' <% p0.name) OR p0.manufacturer_id = ANY (ARRAY[]::uuid[]))
);
EXPLAIN (ANALYZE, COSTS OFF, BUFFERS OFF)
SELECT count(*)::int
FROM product_variants AS p
INNER JOIN products AS p1 ON p.product_id = p1.id
INNER JOIN manufacturers AS m ON p1.manufacturer_id = m.id
WHERE p.product_id IN (
    SELECT p0.id
    FROM products AS p0
    WHERE p0.category_id = ANY (ARRAY['01a0e6d1-5271-7537-9381-09052812bae8','01a0e6d1-5272-7ac8-9787-0ff9786ed6f1','01a0e6d1-5272-7be6-b093-5e0d24d629ae','01a0e6d1-5272-7c07-8f79-0f70777380ba']::uuid[]) AND (p0.name ILIKE '%Syngenta%' ESCAPE '\' OR ('Syngenta' <% p0.name) OR p0.manufacturer_id = ANY (ARRAY['01a0e6d1-5270-777c-a58c-8b4a5dabe920']::uuid[]))
);
ROLLBACK;
