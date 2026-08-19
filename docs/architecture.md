# Commerce Operations Platform 아키텍처

사용 기술의 역할과 기능별 적용은 [기술 스택과 기능별 적용](technology-stack.md)을, 테이블별 컬럼, 인덱스, 제약조건과 ERD는 [데이터베이스 구조](database-schema.md)를 참고한다.

## 데이터 소유권

하나의 MySQL 인스턴스에 `commerce_operations`와 `commerce_order_engine` 데이터베이스를 분리한다. 각 서비스 계정은 자기 데이터베이스에만 권한이 있다. C#은 명시적 SQL 마이그레이션 러너를 사용하고 Java는 Flyway를 사용한다. 서비스 간 데이터 공유는 REST 계약으로만 구현한다.

## Phase 2 인증 계약

운영자 인증은 C# Operations API가 소유한다.

- `POST /api/auth/login`: 이메일과 비밀번호를 검증하고 JWT access token을 반환한다.
- `GET /api/auth/me`: Bearer token을 검증하고 현재 운영자 정보를 반환한다.
- 비밀번호는 PBKDF2-SHA256(무작위 salt, 210,000회)으로 저장한다.
- JWT는 HS256으로 서명하며 비밀키와 초기 운영자 정보는 `.env.local`에서만 주입한다.
- React는 Docker 환경에서 Nginx `/api` reverse proxy를 사용하고 개발 서버에서는 Vite proxy를 사용한다.
- 브라우저 토큰은 현재 로컬 개발 범위에서 `sessionStorage`에 저장하며 브라우저 종료 시 제거된다.

초기 운영자는 애플리케이션 시작 시 이메일을 기준으로 생성한다. 이미 존재하는 계정의 비밀번호는 자동 변경하지 않는다. 실제 운영 환경에서는 초기 계정 생성 절차, 키 회전, refresh token 및 감사 로그를 별도로 설계해야 한다.

RabbitMQ 업무 메시지와 Redis 분산 락은 연결 환경만 준비되어 있으며 실제 업무 흐름은 후속 Phase에서 정의한다.

## Phase 3 상품 관리 계약

상품 원장은 C# Operations API와 `commerce_operations.products` 테이블이 소유한다. 주문 엔진은 이 테이블에 직접 접근하지 않는다.

- `GET /api/products`: 검색, 상태 필터, 페이지네이션 목록
- `GET /api/products/{id}`: 단건 조회
- `POST /api/products`: ADMIN 상품 등록
- `PUT /api/products/{id}`: ADMIN 상품 수정
- `PATCH /api/products/{id}/status`: ADMIN 활성·비활성 변경

SKU는 대문자로 정규화하며 고유 인덱스로 중복을 방지한다. 가격은 음수를 허용하지 않고 DB에는 `DECIMAL(18,2)`로 저장한다. 상태는 `ACTIVE` 또는 `INACTIVE`만 허용한다. 목록 정렬 컬럼은 서버의 허용 목록에서만 선택해 SQL 식별자 주입을 차단한다. React는 TanStack Query로 목록 캐시와 변경 후 무효화를 관리한다.

## Phase 4 회원 관리 계약

회원 원장은 C# Operations API와 `commerce_operations.customers` 테이블이 소유한다.

- `GET /api/customers`: 이메일·이름·전화번호 검색, 상태 필터와 페이지네이션
- `GET /api/customers/{id}`: 단건 조회
- `POST /api/customers`: ADMIN 회원 등록
- `PUT /api/customers/{id}`: ADMIN 회원 수정
- `PATCH /api/customers/{id}/status`: ADMIN 상태 변경

이메일은 소문자로 정규화하고 고유 인덱스로 중복을 막는다. 전화번호는 구분 문자를 제거해 숫자와 선택적 선행 `+`만 저장한다. 상태는 `ACTIVE`, `SUSPENDED`, `WITHDRAWN`만 허용한다. 탈퇴는 물리 삭제하지 않고 상태로 관리한다. 로그에는 회원 요청 본문을 기록하지 않는다.
