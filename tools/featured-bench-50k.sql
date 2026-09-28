BEGIN;
INSERT INTO products (id, name, description, category_id, manufacturer_id, is_featured)
SELECT uuidv7(), 'Синт товар ' || g, NULL, '01a0e6d1-5272-7ac8-9787-0ff9786ed6f1', (SELECT id FROM manufacturers ORDER BY id OFFSET (g % 9) LIMIT 1), g % 1000 = 0
FROM generate_series(1, 25000) g;
INSERT INTO product_variants (id, product_id, sku, slug, price, stock_quantity, packaging_unit, packaging_value, packaging_key)
SELECT uuidv7(), p.id, 'SYN-' || row_number() OVER () || '-' || k, 'syn-' || row_number() OVER () || '-' || k, round((10 + random()*500)::numeric, 2), 5, 'Milliliter', 100 * k, (100*k) || '-milliliter'
FROM products p CROSS JOIN generate_series(1, 2) k WHERE p.name LIKE 'Синт товар %';
ANALYZE products; ANALYZE product_variants;
SELECT (SELECT count(*) FROM products WHERE is_featured) featured, (SELECT count(*) FROM product_variants) variants;
EXPLAIN (ANALYZE, COSTS OFF, BUFFERS OFF, TIMING ON)
SELECT s.c, s.c0, s.c1, s.c2, s.id, s.name, s.name0, p8.packaging_key, p8.packaging_unit, p8.packaging_value, p10.price
FROM (
    SELECT (
        SELECT p1.id
        FROM product_variants AS p1
        WHERE p1.product_id = p.id
        ORDER BY p1.price, p1.id
        LIMIT 1) AS c, (
        SELECT p2.sku
        FROM product_variants AS p2
        WHERE p2.product_id = p.id
        ORDER BY p2.price, p2.id
        LIMIT 1) AS c0, (
        SELECT p3.slug
        FROM product_variants AS p3
        WHERE p3.product_id = p.id
        ORDER BY p3.price, p3.id
        LIMIT 1) AS c1, (
        SELECT p6.stock_quantity
        FROM product_variants AS p6
        WHERE p6.product_id = p.id
        ORDER BY p6.price, p6.id
        LIMIT 1) AS c2, p.id, p.name, m.name AS name0
    FROM products AS p
    INNER JOIN manufacturers AS m ON p.manufacturer_id = m.id
    WHERE p.is_featured AND EXISTS (
        SELECT 1
        FROM product_variants AS p0
        WHERE p0.product_id = p.id)
    ORDER BY p.id DESC
    LIMIT 12
) AS s
LEFT JOIN (
    SELECT p7.packaging_key, p7.packaging_unit, p7.packaging_value, p7.product_id
    FROM (
        SELECT p4.packaging_key, p4.packaging_unit, p4.packaging_value, p4.product_id, ROW_NUMBER() OVER(PARTITION BY p4.product_id ORDER BY p4.price, p4.id) AS row
        FROM product_variants AS p4
    ) AS p7
    WHERE p7.row <= 1
) AS p8 ON s.id = p8.product_id
LEFT JOIN (
    SELECT p9.price, p9.product_id
    FROM (
        SELECT p5.price, p5.product_id, ROW_NUMBER() OVER(PARTITION BY p5.product_id ORDER BY p5.price, p5.id) AS row
        FROM product_variants AS p5
    ) AS p9
    WHERE p9.row <= 1
) AS p10 ON s.id = p10.product_id
ORDER BY s.id DESC;
EXPLAIN (ANALYZE, COSTS OFF, BUFFERS OFF)
SELECT v.id, v.sku, v.slug, v.stock_quantity, v.packaging_key, v.packaging_unit, v.packaging_value, v.price, p.id, p.name, m.name
FROM products p JOIN manufacturers m ON m.id = p.manufacturer_id
JOIN LATERAL (SELECT * FROM product_variants v WHERE v.product_id = p.id ORDER BY v.price, v.id LIMIT 1) v ON true
WHERE p.is_featured ORDER BY p.id DESC LIMIT 12;
ROLLBACK;
