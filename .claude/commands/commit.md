---
title: COMMIT
description: Conventional Commits 규격에 따라 변경사항을 커밋합니다.
argument_hint: "[주제]를 전달하면 해당 주제 관련 파일만 선별하여 커밋합니다"
---

# Git 커밋 규칙

Conventional Commits 규격을 따르며, 변경사항을 명확하고 일관된 규칙에 따라 커밋합니다.

## 주제 파라미터 (`$ARGUMENTS`)

**주제가 지정된 경우:** $ARGUMENTS

주제 파라미터를 사용하면 특정 주제와 관련된 변경사항만 선별하여 커밋할 수 있습니다.

**사용 예시:**
```
/commit Calculator        # Calculator 관련 변경만 커밋
/commit 테스트 리팩터링    # 테스트 리팩터링 관련 변경만 커밋
/commit API 엔드포인트     # API 엔드포인트 관련 변경만 커밋
```

## Conventional Commits 형식

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### 커밋 타입 (type)

| 타입 | 설명 | 예시 |
|------|------|------|
| `feat` | 새로운 기능 추가 | `feat: 사용자 로그인 기능 추가` |
| `fix` | 버그 수정 | `fix: null 참조 예외 처리` |
| `docs` | 문서 변경 | `docs: README 설치 가이드 추가` |
| `style` | 코드 포맷팅 (동작 변경 없음) | `style: 들여쓰기 통일` |
| `refactor` | 리팩터링 (기능/버그 수정 아님) | `refactor: 중복 코드 메서드로 추출` |
| `perf` | 성능 개선 | `perf: 쿼리 최적화` |
| `test` | 테스트 추가/수정 | `test: 로그인 실패 케이스 추가` |
| `build` | 빌드 시스템/의존성 변경 | `build: NuGet 패키지 업데이트` |
| `ci` | CI 설정 변경 | `ci: GitHub Actions 워크플로우 추가` |
| `chore` | 기타 변경 (src/test 미포함) | `chore: .gitignore 업데이트` |

### 스코프 (scope)

영향 받는 코드 영역을 괄호 안에 명시합니다 (선택사항):

```
feat(auth): 소셜 로그인 지원 추가
fix(api): 타임아웃 오류 처리
refactor(calculator): 연산 로직 분리
```

### Breaking Changes

호환성을 깨는 변경은 `!`를 타입 뒤에 추가하거나 푸터에 명시합니다:

```
feat!: API 응답 형식 변경

BREAKING CHANGE: 응답 JSON 구조가 변경되었습니다.
```

## 커밋 메시지 작성 규칙

1. **제목 (첫 줄)**
   - 72자 이내로 작성
   - 명령형으로 작성 ("추가한다" X, "추가" O)
   - 마침표 사용 금지
   - 한글로 작성

2. **본문 (선택)**
   - 제목과 빈 줄로 구분
   - "무엇을", "왜" 변경했는지 설명
   - 72자마다 줄바꿈 권장

3. **푸터 (선택)**
   - Breaking Change 정보
   - 관련 이슈 참조 (예: `Closes #123`)

## 커밋 메시지 예시

### 새 기능 추가
```
feat(calculator): 나눗셈 기능 구현

- Divide 메서드 추가
- 0으로 나누기 예외 처리 포함
```

### 버그 수정
```
fix(auth): 토큰 만료 시 자동 갱신 실패 수정

만료된 리프레시 토큰으로 갱신 시도 시
무한 루프에 빠지는 문제 해결

Closes #42
```

### 리팩터링
```
refactor: 테스트 픽스처에 공통 설정 추출

각 테스트 클래스에서 중복된 초기화 코드를
BaseTestFixture로 이동
```

### 문서 수정
```
docs: API 엔드포인트 문서 추가
```

### 빌드/의존성
```
build: LanguageExt.Core 4.4.9 패키지 추가
```

## 커밋 조건

다음 조건을 **모두** 충족할 때만 커밋하십시오:
- 모든 테스트가 통과했을 때 (코드 변경 시)
- 컴파일 오류가 없을 때
- 변경이 하나의 논리적 작업 단위를 나타낼 때

## 커밋 원칙

1. **원자적 커밋**: 하나의 커밋은 하나의 논리적 변경만 포함
2. **작고 잦은 커밋**: 크고 드문 커밋보다 작고 잦은 커밋 선호
3. **리팩터링 분리**: 기능 변경과 리팩터링은 별도 커밋으로 분리

## 커밋 절차

1. `git status`로 변경사항 확인
2. `git diff`로 변경 내용 검토
3. `git log --oneline -5`로 최근 커밋 스타일 확인
4. 논리적 단위로 분리하여 스테이징 및 커밋

**주제 파라미터 사용 시:**
- 지정된 주제(`$ARGUMENTS`)와 관련된 파일만 선별하여 스테이징
- 주제와 무관한 파일은 이번 커밋에서 제외

## 완료 메시지

커밋 완료 시 다음 형식으로 표시합니다:

```
✅ 커밋 완료

커밋 정보:
  - 타입: feat/fix/docs/style/refactor/perf/test/build/ci/chore
  - 메시지: {커밋 메시지 첫 줄}
  - 변경 파일: N개
```

## 참고 자료

- [Conventional Commits 1.0.0](https://www.conventionalcommits.org/en/v1.0.0/)
- [Claude Code Best Practices](https://www.anthropic.com/engineering/claude-code-best-practices)
