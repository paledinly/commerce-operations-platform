CREATE TABLE IF NOT EXISTS products (
  id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  sku VARCHAR(50) NOT NULL,
  name VARCHAR(200) NOT NULL,
  price DECIMAL(18,2) NOT NULL,
  status VARCHAR(20) NOT NULL,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY uq_products_sku (sku),
  KEY ix_products_name (name),
  KEY ix_products_status (status),
  CONSTRAINT ck_products_price CHECK (price >= 0),
  CONSTRAINT ck_products_status CHECK (status IN ('ACTIVE', 'INACTIVE'))
);

