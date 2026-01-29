# Try 모나드 학습 코드 추가 계획

## 현재 상태

- **Validation**: Chapter07에 이미 구현됨 (유지)
- **Try**: 구현 필요

---

## Try 모나드 추가 계획

### 위치 및 구조
**권장: Chapter04_FinAndError에 추가** (Option A)

Try는 `Func<Fin<A>>`를 래핑하므로 Fin과 밀접하게 연관됨. 별도 챕터 생성 시 기존 6개 챕터 번호 변경 필요.

```
Step1/Part1_Fundamentals/Chapter04_FinAndError/
  - E01_ErrorTypes.cs        (기존)
  - E02_FinBasics.cs         (기존)
  - E03_ErrorCombining.cs    (기존)
  - E04_TryBasics.cs         (신규)
  - E05_TryComposition.cs    (신규)
  - E06_TryRealWorld.cs      (신규)
  - Exercise03_ApiResults.cs (기존)
  - Exercise04_TryPipeline.cs (신규)
```

---

## 파일별 상세 내용

### E04_TryBasics.cs - Try 기본

**학습 목표:**
- Try<A>의 개념 (Func<Fin<A>> 래핑)
- 지연 평가 (lazy evaluation) 특성
- Try 생성 (Succ, Fail, lift)
- Run()으로 Fin<A> 얻기
- 예외 자동 캐치 및 Error 변환

**섹션:**
1. Try vs Fin 비교
2. Try 성공/실패 생성 (Try.Succ, Try.Fail)
3. Try.lift - 예외 던지는 함수를 Try로 래핑
4. Run() - 지연된 계산 실행
5. 예외 처리 예제 (전통적 try-catch vs Try)

**핵심 예제:**
```csharp
// 예외를 던지는 함수를 Try로 래핑
var tryParse = Try.lift(() => int.Parse("not a number"));
Fin<int> result = tryParse.Run();  // Fail(Error: FormatException)

// 지연 평가 확인
var lazyTry = Try.lift(() => { Console.WriteLine("실행!"); return 42; });
// 아직 출력 없음
lazyTry.Run();  // "실행!" 출력
```

---

### E05_TryComposition.cs - Try 합성

**학습 목표:**
- Map, Bind로 Try 체이닝
- LINQ 구문 (from...select)
- Match, IfFail로 결과 처리
- Catch로 에러 복구
- | 연산자 (Alternative) 폴백 패턴

**섹션:**
1. Map으로 값 변환
2. Bind로 Try 체이닝
3. LINQ 쿼리 구문
4. Match와 IfFail
5. Catch - 에러 복구
6. | 연산자 (Choice/Alternative) - 폴백

**핵심 예제:**
```csharp
// LINQ 구문
var result =
    from x in Try.lift(() => int.Parse("10"))
    from y in Try.lift(() => int.Parse("20"))
    select x + y;

// | 연산자 폴백
var value =
    GetFromCache()
    | GetFromDatabase()
    | Try.Succ(DefaultValue);
```

---

### E06_TryRealWorld.cs - 실무 Try 활용

**학습 목표:**
- 기존 try-catch 대비 장점
- 합성 가능한 에러 처리 파이프라인
- 파일 I/O, JSON 파싱 예제
- ToOption, ToEither, ToFin 변환
- BiBind로 양방향 처리

**섹션:**
1. 기존 try-catch vs Try 비교
2. 파일 I/O 예제
3. JSON 파싱 파이프라인
4. 재시도 패턴
5. 타입 변환 (ToOption, ToEither, ToFin)
6. BiBind - 양방향 바인딩

**핵심 예제:**
```csharp
// 중첩 try-catch 대신 평탄한 파이프라인
var config =
    from data in Try.lift(() => File.ReadAllText("config.json"))
    from json in Try.lift(() => JsonSerializer.Deserialize<Config>(data))
    select ValidateConfig(json);

// 재시도 패턴
var withRetry =
    Try.lift(() => HttpCall())
    | Try.lift(() => HttpCall())  // 재시도
    | Try.Succ(DefaultResponse);
```

---

### Exercise04_TryPipeline.cs - 실습

**시나리오:** 설정 파일 로드 시스템

**요구사항:**
1. 파일 읽기 (FileNotFoundException 가능)
2. JSON 파싱 (JsonException 가능)
3. 설정 검증
4. 기본값 폴백 구현
5. 에러 로깅

**테스트 케이스:**
- 정상 파일 로드
- 파일 없음 -> 기본 설정
- JSON 오류 -> 기본 설정
- 검증 실패 -> 에러 메시지

---

## Program.cs 업데이트

`RunChapter04Menu()`에 추가:
```csharp
(5, "E04: Try 기본"),
(6, "E05: Try 합성"),
(7, "E06: 실무 Try 활용"),
(8, "Exercise04: 예외 안전 파이프라인 (실습)")
```

---

## 수정 필요 파일

| 파일 | 작업 |
|------|------|
| E04_TryBasics.cs | 신규 생성 |
| E05_TryComposition.cs | 신규 생성 |
| E06_TryRealWorld.cs | 신규 생성 |
| Exercise04_TryPipeline.cs | 신규 생성 |
| Program.cs | Chapter04 메뉴에 Try 항목 추가 |

---

## 검증 방법

1. `dotnet build` - 빌드 성공 확인
2. 프로그램 실행하여 각 예제 테스트
3. Chapter04 메뉴에서 Try 관련 항목 선택 가능 확인
