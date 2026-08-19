CREATE TABLE IF NOT EXISTS customers (
  id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  email VARCHAR(255) NOT NULL,
  name VARCHAR(100) NOT NULL,
  phone VARCHAR(20) NOT NULL,
  status VARCHAR(20) NOT NULL,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY uq_customers_email (email),
  KEY ix_customers_name (name),
  KEY ix_customers_phone (phone),
  KEY ix_customers_status (status),
  CONSTRAINT ck_customers_status CHECK (status IN ('ACTIVE', 'SUSPENDED', 'WITHDRAWN'))
);

