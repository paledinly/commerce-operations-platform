# Commerce Operations Platform

로컬 PC에서 .NET 운영 API, Java 주문 엔진, React 운영 UI와 MySQL·Redis·RabbitMQ를 함께 실행하는 통합 개발 워크스페이스다. 현재 Phase 1~12 전체 단계(실행 기반부터 운영 안정화까지)를 제공한다.

## 구성

- `commerce-operations-dotnet`: 운영자 인증, 상품·회원, 감사 로그와 Java 주문 엔진 API 게이트웨이
- `commerce-order-engine-java`: 재고·주문·결제·배송·정산 원장과 RabbitMQ 주문 이벤트
- `commerce-operations-react`: 운영 UI
- `commerce-platform-infra`: Docker Compose
- `docs`: 아키텍처와 데이터베이스 문서
- `scripts`: 로컬 실행·검증 스크립트

주요 문서: [기술 스택과 기능별 적용](docs/technology-stack.md), [아키텍처](docs/architecture.md), [데이터베이스 구조](docs/database-schema.md), [Phase 5 재고](docs/phase5-inventory.md), [Phase 6 주문](docs/phase6-orders.md), [Phase 7 결제·이벤트](docs/phase7-payments-events.md), [Phase 8 배송](docs/phase8-shipments.md), [Phase 9 대시보드](docs/phase9-dashboard.md), [Phase 10 정산](docs/phase10-settlements.md), [Phase 11 감사 로그](docs/phase11-audit-logs.md), [Phase 12 운영 안정화](docs/phase12-operational-hardening.md)

## 준비와 실행

.NET SDK 8, JDK 21, Node.js LTS, Docker Desktop이 필요하다. `.env.local.example`을 `.env.local`로 복사하고 로컬 값을 설정한 뒤 실행한다.

```powershell
./scripts/start-local.ps1
```

직접 실행하려면 다음 명령을 사용한다.

```powershell
docker compose -f commerce-platform-infra/compose.yml --env-file .env.local up --build
```

기본 접속 주소:

- 운영 UI: `http://localhost:3000`
- C# Swagger: `http://localhost:5000/swagger`
- Java Swagger: `http://localhost:8080/swagger-ui/index.html`
- RabbitMQ 관리: `http://localhost:15672`

운영 UI는 `.env.local`의 `INITIAL_ADMIN_EMAIL`, `INITIAL_ADMIN_PASSWORD`로 로그인한다. 초기 계정은 C# API 시작 시 없을 때만 안전한 비밀번호 해시로 생성된다.

## 구현 기능

- 상품: 검색, 상태 필터, 페이지 이동, 등록·수정, 활성/비활성 전환
- 회원: 검색, 상태 필터, 페이지 이동, 등록·수정, 상태 변경
- 재고: 상품별 재고 생성, 가용 수량 조정, 변경 이력 조회, 음수 재고 차단
- 주문: 활성 회원·상품 주문, 재고 예약, 주문 상세, 취소와 예약 해제
- 결제: 로컬 승인·전액 환불, 거래 기록, RabbitMQ 주문 이벤트
- 배송: 운송사·송장 등록, 예약 재고 소진, 배송 완료
- 대시보드: 상품·회원·주문·순매출·재고·이벤트 KPI와 상태 차트
- 정산: UTC 일별 승인·환불·순매출 집계, 자동·수동 재집계
- 감사 로그: 운영자 변경 요청의 자원·결과·처리시간 추적

재고·주문·결제·배송 데이터는 Java 주문 엔진이 소유한다. React는 C# `/api`만 호출하고 C#이 내부 키로 Java REST API를 호출한다. 실제 PG와 외부 택배사 연동은 이후 단계 범위다.

## 검증과 종료

```powershell
./scripts/verify-all.ps1
./scripts/check-local-health.ps1
./scripts/stop-local.ps1
```

데이터까지 초기화하는 `reset-local.ps1`은 Docker 볼륨을 제거하므로 필요한 데이터가 없을 때만 사용한다.
