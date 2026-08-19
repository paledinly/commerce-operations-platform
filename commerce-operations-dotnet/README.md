# Commerce Operations .NET

.NET 8 운영자 API다. 운영자 JWT 인증, 상품·회원 관리, 감사 로그를 소유하며 재고·주문·결제·배송·대시보드·정산 요청은 Java 주문 엔진 내부 API로 전달한다. Java 서비스의 데이터베이스를 직접 조회하지 않는다.

`dotnet restore`, `dotnet build`, `dotnet test`, `dotnet run --project src/Commerce.Operations.Api` 순서로 검증·실행한다. Swagger는 `/swagger`, 프로세스 생존 확인은 `/health/live`, MySQL과 Java를 포함한 준비 상태는 `/health/ready`다. 시작 시 `Migrations/*.sql`을 파일명 순서로 적용하며 JWT, 초기 운영자와 내부 API 키 설정은 워크스페이스 `.env.local`을 사용한다.

전체 기술 설명은 [기술 스택과 기능별 적용](../docs/technology-stack.md)을 참고한다.
