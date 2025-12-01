---
title: PUBLISH
description: git tag를 이용해 이전 배포 후부터 변경된 commit 내용을 기반으로 release note를 생성합니다.
argument_hint: "[버전]을 전달합니다 (예: v1.0.0, major, minor, patch)"
---

# 릴리스 노트 생성 규칙

Semantic Versioning과 Conventional Commits를 기반으로 **사용자 친화적인** 릴리스 노트를 생성합니다.

## 버전 파라미터 (`$ARGUMENTS`)

**버전이 지정된 경우:** $ARGUMENTS

| 형식 | 설명 | 예시 |
|------|------|------|
| `vX.Y.Z` | 명시적 버전 | `v1.2.3` |
| `major` | 주 버전 증가 | `v1.0.0` -> `v2.0.0` |
| `minor` | 부 버전 증가 | `v1.0.0` -> `v1.1.0` |
| `patch` | 수 버전 증가 | `v1.0.0` -> `v1.0.1` |

**버전 미지정 시 자동 결정:**
- Breaking Change (`!`) → major
- `feat` 타입 → minor
- 그 외 → patch

## 릴리스 노트 작성 원칙

### 1. 사용자 관점으로 작성

커밋 메시지를 그대로 나열하지 않고, **사용자가 이해할 수 있는 언어**로 변환합니다.

**나쁜 예 (커밋 나열):**
```
- feat(calculator): implement division operation
- fix: null reference exception in Calculator.Divide
- refactor: extract common test setup
```

**좋은 예 (사용자 관점):**
```
### New Features
- **Calculator**: Added division operation with zero-division protection

### Bug Fixes
- Fixed crash when dividing with uninitialized values
```

### 2. 주요 변경사항 강조

릴리스의 **핵심 테마**를 도입부에 요약합니다.

### 3. 컨텍스트 제공

- 왜 이 변경이 필요했는지 설명
- 관련 이슈 번호 링크 (#123)
- 마이그레이션 가이드 (Breaking Changes 시)

### 4. 기여자 인정

새로운 기여자나 주요 기여자를 acknowledgements 섹션에 표시합니다.

## 릴리스 노트 생성 절차

### 1. 상태 확인
```bash
git status
```

### 2. 이전 버전 및 변경 커밋 수집
```bash
git tag --sort=-v:refname | head -1
git log <이전태그>..HEAD --oneline
```

### 3. 변경사항 분석 및 그룹화

커밋을 분석하여 다음 기준으로 그룹화합니다:

| 섹션 | 설명 | Conventional Commits |
|------|------|---------------------|
| Highlights | 이번 릴리스의 핵심 변경사항 | 주요 feat, fix 선별 |
| Breaking Changes | 호환성 변경 및 마이그레이션 가이드 | `!`, BREAKING CHANGE |
| New Features | 새로운 기능 | `feat` |
| Bug Fixes | 버그 수정 | `fix` |
| Performance | 성능 개선 | `perf` |
| Other Changes | 기타 (docs, refactor, test, build, ci, chore) | 나머지 |

### 4. 릴리스 노트 작성

파일 경로: `.release-notes/v{버전}.md`

### 5. Git 태그 생성
```bash
git tag -a v{버전} -m "Release v{버전}"
```

## 릴리스 노트 템플릿

```markdown
# v{버전}

> Released on {YYYY-MM-DD}

## Highlights

이번 릴리스의 주요 변경사항을 1-3문장으로 요약합니다.

{주요 변경사항에 대한 간략한 설명}

## Breaking Changes

> **Migration Guide**: 업그레이드 시 필요한 변경사항

### {변경 제목}

**Before:**
```csharp
// 이전 코드
```

**After:**
```csharp
// 변경된 코드
```

## New Features

### {기능 제목}

{기능에 대한 설명}

```csharp
// 사용 예시
```

## Bug Fixes

- **{영향 범위}**: {수정 내용 설명} (#이슈번호)

## Performance

- {성능 개선 내용}

## Other Changes

- {기타 변경사항}

## Contributors

이번 릴리스에 기여해 주신 분들께 감사드립니다:

- @{username}

---

**Full Changelog**: [{이전버전}...v{버전}](https://github.com/hhko/Functorium1/compare/{이전버전}...v{버전})
```

## 섹션 작성 가이드

### Highlights

- 사용자에게 가장 중요한 1-3가지 변경사항
- 마케팅 관점에서 작성
- 기술적 세부사항보다 **가치** 강조

### Breaking Changes

- 반드시 **마이그레이션 가이드** 포함
- Before/After 코드 예시 제공
- 영향 범위 명시

### New Features

- **사용 방법**과 **예시 코드** 포함
- 관련 문서 링크 제공
- 스크린샷/다이어그램 (해당 시)

### Bug Fixes

- **영향 범위** 명시 (어떤 상황에서 발생했는지)
- 이슈 번호 링크
- 수정 내용을 사용자 관점에서 설명

### Other Changes

- 사용자에게 직접적 영향이 없는 변경사항
- 간결하게 나열
- 빈 경우 섹션 생략

## 완료 메시지

```
🚀 릴리스 준비 완료

릴리스 정보:
  - 버전: v{버전}
  - 이전 버전: v{이전버전}
  - 포함 커밋: N개

변경사항 요약:
  - Breaking Changes: N개
  - New Features: N개
  - Bug Fixes: N개
  - Other: N개

생성된 파일:
  - .release-notes/v{버전}.md

다음 단계:
  1. 릴리스 노트 검토 및 수정
  2. 커밋: git add .release-notes && git commit -m "docs: v{버전} 릴리스 노트"
  3. 푸시: git push origin main --tags
```

## 릴리스 노트 예시

```markdown
# v1.2.0

> Released on 2025-12-02

## Highlights

이번 릴리스에서는 **나눗셈 연산**이 추가되었으며, 0으로 나누기 시 발생하던
크래시 문제가 해결되었습니다.

## New Features

### Calculator: Division Operation

Calculator 클래스에 나눗셈 기능이 추가되었습니다.

```csharp
var calculator = new Calculator();
var result = calculator.Divide(10, 2); // Returns 5
```

0으로 나누기 시도 시 `DivideByZeroException`이 발생합니다.

## Bug Fixes

- **Calculator**: 초기화되지 않은 값으로 연산 시 발생하던 NullReferenceException 수정 (#42)

## Other Changes

- 테스트 커버리지 85%로 개선
- CI 파이프라인에 코드 커버리지 리포트 추가

## Contributors

- @hhko

---

**Full Changelog**: [v1.1.0...v1.2.0](https://github.com/hhko/Functorium1/compare/v1.1.0...v1.2.0)
```

## 참고 자료

- [Serilog Release Notes](https://github.com/serilog/serilog/releases)
- [.NET Release Notes](https://github.com/dotnet/core/tree/main/release-notes)
- [Writing Better Release Notes](https://simonwillison.net/2022/Jan/31/release-notes/)
