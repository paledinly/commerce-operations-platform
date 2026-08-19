ALTER TABLE orders DROP CHECK ck_orders_status;
ALTER TABLE orders ADD CONSTRAINT ck_orders_status CHECK (status IN ('CREATED','PAID','SHIPPED','COMPLETED','CANCELLED','REFUNDED'));
ALTER TABLE inventory_movements DROP CHECK ck_inventory_movements_type;
ALTER TABLE inventory_movements ADD CONSTRAINT ck_inventory_movements_type CHECK (movement_type IN ('INITIAL','ADJUSTMENT','RESERVATION','RELEASE','FULFILLMENT'));
CREATE TABLE shipments (
  id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  order_id BIGINT NOT NULL,
  carrier VARCHAR(100) NOT NULL,
  tracking_number VARCHAR(100) NOT NULL,
  status VARCHAR(20) NOT NULL,
  shipped_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  delivered_at TIMESTAMP NULL,
  UNIQUE KEY uq_shipments_order(order_id),
  UNIQUE KEY uq_shipments_tracking(carrier,tracking_number),
  CONSTRAINT fk_shipments_order FOREIGN KEY(order_id) REFERENCES orders(id),
  CONSTRAINT ck_shipments_status CHECK(status IN('SHIPPED','DELIVERED'))
);
