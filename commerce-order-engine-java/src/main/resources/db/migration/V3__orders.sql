ALTER TABLE inventory_movements DROP CHECK ck_inventory_movements_type;
ALTER TABLE inventory_movements ADD CONSTRAINT ck_inventory_movements_type CHECK (movement_type IN ('INITIAL','ADJUSTMENT','RESERVATION','RELEASE'));

CREATE TABLE orders (
  id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  customer_id BIGINT NOT NULL,
  customer_email VARCHAR(255) NOT NULL,
  customer_name VARCHAR(100) NOT NULL,
  status VARCHAR(20) NOT NULL,
  total_amount DECIMAL(18,2) NOT NULL,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  KEY ix_orders_customer_created (customer_id, created_at),
  KEY ix_orders_status_created (status, created_at),
  CONSTRAINT ck_orders_status CHECK (status IN ('CREATED','CANCELLED')),
  CONSTRAINT ck_orders_total CHECK (total_amount >= 0)
);

CREATE TABLE order_items (
  id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  order_id BIGINT NOT NULL,
  product_id BIGINT NOT NULL,
  sku VARCHAR(50) NOT NULL,
  product_name VARCHAR(200) NOT NULL,
  unit_price DECIMAL(18,2) NOT NULL,
  quantity BIGINT NOT NULL,
  line_amount DECIMAL(18,2) NOT NULL,
  KEY ix_order_items_order (order_id),
  CONSTRAINT fk_order_items_order FOREIGN KEY (order_id) REFERENCES orders(id),
  CONSTRAINT ck_order_items_price CHECK (unit_price >= 0),
  CONSTRAINT ck_order_items_quantity CHECK (quantity > 0),
  CONSTRAINT ck_order_items_amount CHECK (line_amount >= 0)
);
