# Phase 6 주문과 재고 예약

## 제공 기능

- 활성 회원과 활성 상품으로 주문 생성
- 주문 목록과 상품 스냅샷 상세 조회
- 주문 생성 시 가용 재고를 예약 재고로 이동
- 주문 취소 시 예약 재고를 가용 재고로 복원
- 부족 재고 주문 차단과 중복 취소 차단
- 여러 상품은 상품 ID 오름차순으로 Redis 잠금을 획득

결제, 배송, 주문 완료 상태와 RabbitMQ 업무 이벤트는 다음 단계 범위다.

## 소유권과 호출 흐름

React는 JWT와 함께 C# `/api/orders`만 호출한다. C#은 `commerce_operations`에서 회원·상품 상태를 확인하고 가격·이름 스냅샷을 만든 뒤 내부 API 키로 Java를 호출한다. Java는 `orders`, `order_items`, `inventories`, `inventory_movements`를 하나의 트랜잭션에서 변경한다.

## 상태와 재고

- `CREATED`: 주문 수량만큼 `available_quantity` 감소, `reserved_quantity` 증가
- `CANCELLED`: 주문 수량만큼 `reserved_quantity` 감소, `available_quantity` 증가
- 재고 이력 유형: `RESERVATION`, `RELEASE`

## API

- `GET /api/orders?page=1&pageSize=20`
- `GET /api/orders/{id}`
- `POST /api/orders`
- `POST /api/orders/{id}/cancel`

주문 생성 예시:

```json
{"customerId":1,"items":[{"productId":1,"quantity":2}]}
```

Java Flyway `V3__orders.sql`이 `orders`, `order_items`를 만들고 재고 이력 유형을 확장한다.
