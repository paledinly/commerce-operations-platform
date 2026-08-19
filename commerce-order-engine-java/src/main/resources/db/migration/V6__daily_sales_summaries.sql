CREATE TABLE daily_sales_summaries (
  settlement_date DATE NOT NULL PRIMARY KEY,
  payment_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
  refund_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
  net_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
  payment_count BIGINT NOT NULL DEFAULT 0,
  refund_count BIGINT NOT NULL DEFAULT 0,
  calculated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  CONSTRAINT ck_daily_sales_amounts CHECK(payment_amount>=0 AND refund_amount>=0),
  CONSTRAINT ck_daily_sales_counts CHECK(payment_count>=0 AND refund_count>=0)
);
