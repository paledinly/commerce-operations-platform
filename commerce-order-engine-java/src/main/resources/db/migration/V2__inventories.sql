CREATE TABLE inventories (
  product_id BIGINT NOT NULL PRIMARY KEY,
  available_quantity BIGINT NOT NULL DEFAULT 0,
  reserved_quantity BIGINT NOT NULL DEFAULT 0,
  version BIGINT NOT NULL DEFAULT 0,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  CONSTRAINT ck_inventories_available CHECK (available_quantity >= 0),
  CONSTRAINT ck_inventories_reserved CHECK (reserved_quantity >= 0)
);

CREATE TABLE inventory_movements (
  id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  product_id BIGINT NOT NULL,
  movement_type VARCHAR(20) NOT NULL,
  quantity_delta BIGINT NOT NULL,
  available_after BIGINT NOT NULL,
  reason VARCHAR(200) NOT NULL,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  KEY ix_inventory_movements_product_created (product_id, created_at),
  CONSTRAINT fk_inventory_movements_inventory FOREIGN KEY (product_id) REFERENCES inventories(product_id),
  CONSTRAINT ck_inventory_movements_type CHECK (movement_type IN ('INITIAL', 'ADJUSTMENT'))
);

