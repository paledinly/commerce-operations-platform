# Phase 10 일별 매출 정산

## 제공 기능

- UTC 일자별 결제 승인액·건수 집계
- UTC 일자별 환불액·건수 집계
- 승인액에서 환불액을 뺀 순매출 계산
- 매일 UTC 00:10에 최근 3일 자동 재집계
- 최대 367일 기간 조회와 관리자 수동 재집계

실제 PG 정산금 입금이나 외부 회계 시스템 연동은 포함하지 않는다. 원천 데이터는 변경하지 않고 `payment_transactions`를 다시 읽어 결과를 덮어쓰므로 같은 기간을 반복 실행해도 중복 합산되지 않는다.

## 데이터와 API

Flyway `V6__daily_sales_summaries.sql`이 `daily_sales_summaries`를 생성한다. Java 주문 엔진이 결제 원장과 정산 결과를 모두 소유하며 C#은 내부 REST API만 호출한다.

- `GET /api/settlements?from=2026-01-01&to=2026-01-31`
- `POST /api/settlements/rebuild?from=2026-01-01&to=2026-01-31`

조회는 로그인 운영자에게 허용되고 수동 재집계는 `ADMIN` 권한이 필요하다. React `정산` 탭에서 기간, 합계와 일별 상세를 확인할 수 있다.
