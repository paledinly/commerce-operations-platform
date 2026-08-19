# Commerce Operations React

React/TypeScript 운영 UI다. 운영자 로그인, 대시보드, 상품·회원·재고·주문, 결제·환불·배송, 일별 정산과 감사 로그 화면을 제공한다. React는 Java 주문 엔진을 직접 호출하지 않고 `/api`를 통해 C# Operations API만 호출한다.

`npm install`, `npm run typecheck`, `npm run lint`, `npm run test`, `npm run build`, `npm run dev`로 검증·실행한다. TanStack Query가 서버 데이터 캐시를, Zustand가 로그인 세션을, React Hook Form과 Zod가 폼과 입력 검증을 담당한다. Docker에서는 Nginx가 정적 파일을 제공하고 `/api` 요청을 C# API로 전달한다.

전체 기술 설명은 [기술 스택과 기능별 적용](../docs/technology-stack.md)을 참고한다.
