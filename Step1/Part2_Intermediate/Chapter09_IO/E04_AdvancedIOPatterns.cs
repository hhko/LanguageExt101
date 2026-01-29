using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter09_IO;

/// <summary>
/// IO 모나드의 고급 패턴을 학습합니다.
///
/// 학습 목표:
/// - 꼬리 재귀(tail)를 사용한 스택 안전한 재귀
/// - Schedule을 사용한 반복과 재시도
/// - repeat와 retry 패턴
/// - yieldFor를 사용한 대기
/// - 재귀적 IO 합성
///
/// 핵심 개념:
/// IO는 재귀적으로 합성할 수 있으며, tail 함수를 사용하면
/// 스택 오버플로우 없이 무한 루프나 깊은 재귀를 수행할 수 있습니다.
/// Schedule을 활용하면 반복, 재시도, 대기 등의 패턴을 선언적으로 표현합니다.
///
/// 참고: language-ext/Samples/IOExamples/Program.cs
/// </summary>
public static class E04_AdvancedIOPatterns
{
    // 콘솔 입출력 헬퍼
    private static IO<string> ReadLine =>
        lift(() => Console.ReadLine() ?? "");

    private static IO<Unit> WriteLine(string line) =>
        lift(() =>
        {
            Console.WriteLine($"    {line}");
            return unit;
        });

    private static IO<Unit> Write(string text) =>
        lift(() =>
        {
            Console.Write($"    {text}");
            return unit;
        });

    private static IO<DateTime> Now =>
        lift(() => DateTime.Now);

    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 09-E04: IO 고급 패턴");

        // ============================================================
        // 1. lift와 Pure - IO 생성의 기초
        // ============================================================
        MenuHelper.PrintSubHeader("1. lift와 Pure - IO 생성의 기초");

        MenuHelper.PrintExplanation("lift()는 부수 효과가 있는 함수를 IO로 래핑합니다.");
        MenuHelper.PrintExplanation("Pure()는 순수한 값을 IO로 래핑합니다 (IO.pure와 동일).");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// lift: 부수 효과를 IO로 래핑
static IO<string> ReadLine => lift(() => Console.ReadLine() ?? """");

static IO<Unit> WriteLine(string line) => lift(() => {
    Console.WriteLine(line);
    return unit;
});

// IO.pure: 순수 값을 IO로
var pureValue = IO.pure(42);  // IO<int>");

        var pureValue = IO.pure(42);
        MenuHelper.PrintResult("IO.pure(42).Run()", pureValue.Run());

        // IO.lift 예시
        var currentTime = IO.lift(() => DateTime.Now);
        MenuHelper.PrintResult("IO.lift(() => DateTime.Now).Run()", currentTime.Run());

        // ============================================================
        // 2. 조건부 IO와 재귀적 합성
        // ============================================================
        MenuHelper.PrintSubHeader("2. 조건부 IO와 재귀적 합성");

        MenuHelper.PrintExplanation("조건에 따라 다른 IO를 선택하고 재귀적으로 합성합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 카운트다운: 재귀적 IO
static IO<Unit> Countdown(int n) =>
    n <= 0
        ? WriteLine(""발사!"")
        : from _ in WriteLine($""{n}..."")
          from r in Countdown(n - 1)
          select unit;");

        IO<Unit> Countdown(int n) =>
            n <= 0
                ? WriteLine("발사!")
                : from _ in WriteLine($"{n}...")
                  from r in Countdown(n - 1)
                  select unit;

        Console.WriteLine("  [카운트다운 시작]");
        Countdown(5).Run();

        // ============================================================
        // 3. tail - 스택 안전한 꼬리 재귀
        // ============================================================
        MenuHelper.PrintSubHeader("3. tail - 스택 안전한 꼬리 재귀");

        MenuHelper.PrintExplanation("tail()은 꼬리 재귀 최적화를 적용하여 스택 오버플로우를 방지합니다.");
        MenuHelper.PrintExplanation("무한 루프나 매우 깊은 재귀에서 필수적입니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// tail을 사용한 스택 안전한 재귀
static IO<Unit> CountUp(int value, int max) =>
    value > max
        ? Pure(unit)
        : from _ in (value % 1000 == 0 ? WriteLine($""{value}"") : Pure(unit))
          from r in tail(CountUp(value + 1, max))  // 꼬리 재귀 최적화
          select unit;");

        IO<Unit> CountUp(int value, int max) =>
            value > max
                ? Pure(unit)
                : from _ in (value % 1000 == 0 ? WriteLine($"{value}") : Pure(unit))
                  from r in tail(CountUp(value + 1, max))
                  select unit;

        Console.WriteLine("  [1000씩 출력하며 5000까지 카운트]");
        CountUp(0, 5000).Run();

        // ============================================================
        // 4. yieldFor - 대기
        // ============================================================
        MenuHelper.PrintSubHeader("4. yieldFor - 대기");

        MenuHelper.PrintExplanation("yieldFor()로 지정된 시간만큼 대기합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 대기 헬퍼
static IO<Unit> Wait(int milliseconds) => yieldFor(milliseconds);

// 시간 출력 후 1초 대기
var timedOperation =
    from t1 in Now
    from _1 in WriteLine($""시작: {t1:HH:mm:ss.fff}"")
    from _2 in yieldFor(500)  // 500ms 대기
    from t2 in Now
    from _3 in WriteLine($""종료: {t2:HH:mm:ss.fff}"")
    select unit;");

        var timedOperation =
            from t1 in Now
            from _1 in WriteLine($"시작: {t1:HH:mm:ss.fff}")
            from _2 in yieldFor(500)
            from t2 in Now
            from _3 in WriteLine($"종료: {t2:HH:mm:ss.fff}")
            select unit;

        timedOperation.Run();

        // ============================================================
        // 5. Schedule - 스케줄 정의
        // ============================================================
        MenuHelper.PrintSubHeader("5. Schedule - 스케줄 정의");

        MenuHelper.PrintExplanation("Schedule은 반복, 재시도, 지연의 패턴을 정의합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 주요 Schedule 패턴
Schedule.Once           // 1회만
Schedule.Forever        // 무한 반복
Schedule.recurs(5)      // 5회 반복
Schedule.spaced(1000)   // 1초 간격으로 무한 반복
Schedule.fixedInterval(TimeSpan.FromSeconds(1))  // 고정 간격

// 조합
Schedule.recurs(3) | Schedule.spaced(500)  // 500ms 간격으로 3회");

        MenuHelper.PrintExplanation("Schedule은 repeat, retry 등과 함께 사용됩니다.");

        // ============================================================
        // 6. repeat - 스케줄 기반 반복
        // ============================================================
        MenuHelper.PrintSubHeader("6. repeat - 스케줄 기반 반복");

        MenuHelper.PrintExplanation("repeat()는 IO를 스케줄에 따라 반복 실행합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 3회 반복
var counter = 0;
var repeating = repeat(
    Schedule.recurs(3),
    from _ in lift(() => { counter++; return unit; })
    from t in Now
    from r in WriteLine($""반복 #{counter} at {t:ss.fff}"")
    select unit
).As();");

        var counter = 0;
        var repeating = repeat(
            Schedule.recurs(3),
            from _ in lift(() => { counter++; return unit; })
            from t in Now
            from r in WriteLine($"반복 #{counter} at {t:ss.fff}")
            select unit
        ).As();

        repeating.Run();

        // 간격을 두고 반복
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode(@"// 200ms 간격으로 3회 반복
var spacedRepeat = repeat(
    Schedule.recurs(3) | Schedule.spaced(200),
    ...
).As();");

        counter = 0;
        var spacedRepeat = repeat(
            Schedule.recurs(3) | Schedule.spaced(200),
            from _ in lift(() => { counter++; return unit; })
            from t in Now
            from r in WriteLine($"간격 반복 #{counter} at {t:ss.fff}")
            select unit
        ).As();

        spacedRepeat.Run();

        // ============================================================
        // 7. retry - 스케줄 기반 재시도
        // ============================================================
        MenuHelper.PrintSubHeader("7. retry - 스케줄 기반 재시도");

        MenuHelper.PrintExplanation("retry()는 실패 시 스케줄에 따라 재시도합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 시뮬레이션: 3번째 시도에서 성공
var attempts = 0;
var retryingOp = retry(
    Schedule.recurs(5),
    lift(() => {
        attempts++;
        if (attempts < 3) throw new Exception($""실패 #{attempts}"");
        return $""성공! (시도 #{attempts})"";
    })
).As();");

        var attempts = 0;
        var retryingOp = retry(
            Schedule.recurs(5),
            WriteLine($"시도 중...")
                .Bind(_ => IO.lift(() =>
                {
                    attempts++;
                    Console.WriteLine($"      (시도 #{attempts})");
                    if (attempts < 3)
                        throw new Exception($"실패 #{attempts}");
                    return $"성공! (시도 #{attempts})";
                }))
        ).As();

        try
        {
            var result = retryingOp.Run();
            MenuHelper.PrintResult("재시도 결과", result);
        }
        catch (Exception ex)
        {
            MenuHelper.PrintError($"최종 실패: {ex.Message}");
        }

        // ============================================================
        // 8. | 연산자 - 에러 핸들링
        // ============================================================
        MenuHelper.PrintSubHeader("8. | 연산자 - 에러 핸들링");

        MenuHelper.PrintExplanation("| 연산자는 IO가 실패할 경우 대체 IO를 실행합니다.");
        MenuHelper.PrintExplanation("(왼쪽이 실패하면 오른쪽 실행)");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 실패 시 대체 IO 실행
var withFallback =
    IO.fail<string>(Error.New(""첫 번째 실패""))
    | IO.pure(""대체 값 사용"");");

        var withFallback =
            IO.fail<string>(LanguageExt.Common.Error.New("첫 번째 실패"))
            | IO.pure("대체 값 사용");

        MenuHelper.PrintResult("| 결과", withFallback.Run());

        // repeat과 | 조합
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode(@"// repeat이 모두 실패하면 대체 메시지 출력
var repeatWithFallback =
    repeat(Schedule.recurs(2),
           IO.fail<Unit>(Error.New(""반복 실패""))).As()
    | WriteLine(""모든 반복이 실패하여 대체 실행"");");

        var repeatWithFallback =
            repeat(Schedule.recurs(2),
                   IO.fail<Unit>(LanguageExt.Common.Error.New("반복 실패"))).As()
            | WriteLine("모든 반복이 실패하여 대체 실행");

        repeatWithFallback.Run();

        // ============================================================
        // 9. 실전 예제: 입력 검증 재시도
        // ============================================================
        MenuHelper.PrintSubHeader("9. 실전 예제: 입력 검증 재시도");

        MenuHelper.PrintExplanation("사용자 입력을 검증하고 실패 시 재시도하는 패턴입니다.");
        MenuHelper.PrintExplanation("(시뮬레이션: 미리 정의된 입력 사용)");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 숫자 입력 요청 (재시도 포함)
static IO<int> ReadNumber(string question) =>
    retry(Schedule.recurs(3),
          from _1 in WriteLine(question)
          from input in ReadLine
          from num in int.TryParse(input, out var v)
                        ? Pure(v)
                        : IO.fail<int>(Error.New(""숫자가 아닙니다!""))
          select num).As();");

        // 시뮬레이션된 입력
        var simulatedInputs = new Queue<string>(new[] { "abc", "xyz", "42" });

        IO<string> SimulatedReadLine() => lift(() =>
        {
            var input = simulatedInputs.Count > 0 ? simulatedInputs.Dequeue() : "0";
            Console.WriteLine($"    [입력: {input}]");
            return input;
        });

        IO<int> ReadNumberSimulated(string question) =>
            retry(Schedule.recurs(5),
                  from _1 in WriteLine(question)
                  from input in SimulatedReadLine()
                  from num in int.TryParse(input, out var v)
                                ? Pure(v)
                                : from _2 in WriteLine("숫자가 아닙니다! 다시 시도...")
                                  from fail in IO.fail<int>(LanguageExt.Common.Error.New("파싱 실패"))
                                  select fail
                  select num).As();

        var numberResult = ReadNumberSimulated("숫자를 입력하세요:").Run();
        MenuHelper.PrintResult("입력된 숫자", numberResult);

        // ============================================================
        // 10. 실전 예제: 두 숫자 더하기 (반복)
        // ============================================================
        MenuHelper.PrintSubHeader("10. 실전 예제: 두 숫자 더하기");

        MenuHelper.PrintExplanation("repeat와 retry를 조합한 실전 패턴입니다.");
        MenuHelper.PrintBlankLines();

        // 시뮬레이션된 입력 리셋
        var addInputs = new Queue<string>(new[] { "10", "20", "5", "15" });

        IO<string> AddSimulatedReadLine() => lift(() =>
        {
            var input = addInputs.Count > 0 ? addInputs.Dequeue() : "0";
            Console.WriteLine($"    [입력: {input}]");
            return input;
        });

        IO<int> ReadNumForAdd(string question) =>
            retry(Schedule.recurs(3),
                  from _1 in WriteLine(question)
                  from input in AddSimulatedReadLine()
                  from num in int.TryParse(input, out var v)
                                ? Pure(v)
                                : IO.fail<int>(LanguageExt.Common.Error.New("숫자 아님"))
                  select num).As();

        var addingProgram = repeat(
            Schedule.recurs(2),
            from x in ReadNumForAdd("첫 번째 숫자:")
            from y in ReadNumForAdd("두 번째 숫자:")
            from _ in WriteLine($"{x} + {y} = {x + y}")
            select unit
        ).As();

        addingProgram.Run();

        MenuHelper.PrintSuccess("IO 고급 패턴 학습 완료!");
    }
}
