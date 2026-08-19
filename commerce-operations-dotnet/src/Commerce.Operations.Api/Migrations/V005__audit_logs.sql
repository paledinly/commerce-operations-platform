CREATE TABLE IF NOT EXISTS audit_logs (
  id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  operator_id BIGINT NULL,
  operator_email VARCHAR(255) NOT NULL,
  http_method VARCHAR(10) NOT NULL,
  request_path VARCHAR(500) NOT NULL,
  resource_type VARCHAR(50) NOT NULL,
  status_code INT NOT NULL,
  duration_ms BIGINT NOT NULL,
  ip_address VARCHAR(64) NULL,
  user_agent VARCHAR(500) NULL,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  KEY ix_audit_logs_created (created_at, id),
  KEY ix_audit_logs_operator_created (operator_id, created_at),
  KEY ix_audit_logs_resource_created (resource_type, created_at),
  KEY ix_audit_logs_status_created (status_code, created_at)
);
