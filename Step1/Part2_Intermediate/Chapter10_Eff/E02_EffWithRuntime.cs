using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Sys;
using LanguageExt.Sys.Traits;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter10_Eff;

/// <summary>
/// Eff&lt;RT, A&gt;와 Runtime 개념을 학습합니다.
///
/// 학습 목표:
/// - Eff&lt;RT, A&gt;와 Runtime 개념
/// - Has&lt;Eff&lt;RT&gt;, Trait&gt; 패턴
/// - Console&lt;RT&gt; 등 Trait 사용법
/// - 커스텀 Runtime 정의
///
/// 핵심 개념:
/// Runtime은 효과(Effect)를 실행하는 데 필요한 환경(의존성)입니다.
/// Has&lt;M, Trait&gt; 패턴으로 필요한 기능을 선언적으로 요청합니다.
/// 이를 통해 테스트 시 Mock Runtime을 주입할 수 있습니다.
/// </summary>
public static class E02_EffWithRuntime
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 10-E02: Eff<RT, A>와 런타임");

        // ============================================================
        // 1. Runtime 개념
        // ============================================================
        MenuHelper.PrintSubHeader("1. Runtime 개념");

        MenuHelper.PrintExplanation("Eff<A>: 런타임 없이 실행 가능한 효과");
        MenuHelper.PrintExplanation("Eff<RT, A>: 런타임 RT가 필요한 효과");
        MenuHelper.PrintExplanation("Runtime은 Console, 시간, 파일 등 외부 의존성을 제공합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// Eff<A>: 단순 효과
var simple = SuccessEff(42);

// Eff<RT, A>: 런타임 필요 효과
// RT가 ConsoleIO trait를 가져야 함
Eff<RT, Unit> greet<RT>() where RT : Has<Eff<RT>, ConsoleIO>
    => Console<RT>.writeLine(""Hello!"");");

        // 단순 Eff<A>
        var simple = SuccessEff(42);
        MenuHelper.PrintResult("SuccessEff(42).Run()", simple.Run());

        // ============================================================
        // 2. Has<M, Trait> 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("2. Has<M, Trait> 패턴");

        MenuHelper.PrintExplanation("Has<Eff<RT>, Trait>: RT가 특정 Trait을 제공함을 선언");
        MenuHelper.PrintExplanation("ConsoleIO: 콘솔 입출력 기능");
        MenuHelper.PrintExplanation("TimeIO: 시간 관련 기능");
        MenuHelper.PrintExplanation("FileIO: 파일 시스템 기능");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// ConsoleIO trait 요구
where RT : Has<Eff<RT>, ConsoleIO>

// 여러 Trait 요구
where RT : Has<Eff<RT>, ConsoleIO>, Has<Eff<RT>, TimeIO>");

        // 실제 사용 예시 설명
        MenuHelper.PrintExplanation("이 제약으로 Runtime이 어떤 기능을 제공해야 하는지 명시합니다.");

        // ============================================================
        // 3. ConsoleIO trait 사용
        // ============================================================
        MenuHelper.PrintSubHeader("3. ConsoleIO trait 사용");

        MenuHelper.PrintExplanation("Console<RT>.writeLine(): 한 줄 출력");
        MenuHelper.PrintExplanation("Console<RT>.readLine: 한 줄 입력");
        MenuHelper.PrintExplanation("Console<RT>.write(): 줄바꿈 없이 출력");
        MenuHelper.PrintBlankLines();

        // 시뮬레이션으로 대체 (실제 콘솔 대신)
        MenuHelper.PrintCode(@"// ConsoleIO 사용 예시
static Eff<RT, Unit> greetUser<RT>() where RT : Has<Eff<RT>, ConsoleIO> =>
    from _  in Console<RT>.writeLine(""이름을 입력하세요:"")
    from nm in Console<RT>.readLine
    from __ in Console<RT>.writeLine($""안녕하세요, {nm}님!"")
    select unit;");

        // 시뮬레이션된 실행 결과
        Console.WriteLine();
        Console.WriteLine("    [시뮬레이션 실행 결과]");
        Console.WriteLine("    > 이름을 입력하세요:");
        Console.WriteLine("    < 홍길동");
        Console.WriteLine("    > 안녕하세요, 홍길동님!");

        // ============================================================
        // 4. 시뮬레이션 Runtime (테스트용)
        // ============================================================
        MenuHelper.PrintSubHeader("4. 시뮬레이션 Runtime");

        MenuHelper.PrintExplanation("테스트 시 실제 콘솔 대신 시뮬레이션 Runtime을 사용합니다.");
        MenuHelper.PrintExplanation("입력 큐와 출력 리스트로 동작을 시뮬레이션합니다.");
        MenuHelper.PrintBlankLines();

        // 시뮬레이션 방식 설명
        MenuHelper.PrintCode(@"// 시뮬레이션 Runtime 개념
record TestRuntime(Queue<string> Inputs, List<string> Outputs)
{
    // ConsoleIO.writeLine: Outputs에 추가
    // ConsoleIO.readLine: Inputs에서 꺼냄
}");

        // 시뮬레이션 예시
        var simulatedInputs = new Queue<string>(["hello", "world"]);
        var simulatedOutputs = new List<string>();

        // 시뮬레이션 실행
        simulatedOutputs.Add("첫 번째 프롬프트");
        var input1 = simulatedInputs.Dequeue();
        simulatedOutputs.Add($"입력 받음: {input1}");
        var input2 = simulatedInputs.Dequeue();
        simulatedOutputs.Add($"입력 받음: {input2}");

        Console.WriteLine("    [시뮬레이션 결과]");
        Console.WriteLine($"    입력 큐: [\"hello\", \"world\"]");
        Console.WriteLine($"    출력:");
        foreach (var output in simulatedOutputs)
            Console.WriteLine($"      - {output}");

        // ============================================================
        // 5. 다중 Trait 조합
        // ============================================================
        MenuHelper.PrintSubHeader("5. 다중 Trait 조합");

        MenuHelper.PrintExplanation("여러 Trait을 요구하는 효과를 정의할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// ConsoleIO + TimeIO 조합
static Eff<RT, Unit> showCurrentTime<RT>()
    where RT : Has<Eff<RT>, ConsoleIO>, Has<Eff<RT>, TimeIO>
{
    return from now in Time<RT>.now
           from _  in Console<RT>.writeLine($""현재 시간: {now}"")
           select unit;
}");

        // 시뮬레이션 실행
        Console.WriteLine();
        Console.WriteLine("    [시뮬레이션 실행 결과]");
        Console.WriteLine($"    > 현재 시간: {DateTime.Now}");

        // 더 복잡한 예시
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode(@"// 복잡한 다중 Trait 예시
static Eff<RT, Unit> timedInteraction<RT>()
    where RT : Has<Eff<RT>, ConsoleIO>, Has<Eff<RT>, TimeIO>
{
    return from start in Time<RT>.now
           from _1   in Console<RT>.writeLine(""작업 시작"")
           from _2   in Time<RT>.sleepFor(1 * second)
           from end  in Time<RT>.now
           from _3   in Console<RT>.writeLine($""소요 시간: {end - start}"")
           select unit;
}");

        // ============================================================
        // 6. local 함수 (Runtime 변환)
        // ============================================================
        MenuHelper.PrintSubHeader("6. local 함수");

        MenuHelper.PrintExplanation("local: 내부 효과에 다른 Runtime을 사용");
        MenuHelper.PrintExplanation("테스트나 격리된 환경에서 유용합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// local 개념
static Eff<OuterRT, A> withLocalRuntime<OuterRT, InnerRT, A>(
    Eff<InnerRT, A> inner,
    Func<OuterRT, InnerRT> transform)
    => local<OuterRT, InnerRT, A>(transform, inner);

// 사용 예시: 외부 RT를 내부 RT로 변환하여 실행
var result = withLocalRuntime(innerEffect, outerRt => outerRt.ToInner());");

        // 개념적 설명
        Console.WriteLine();
        Console.WriteLine("    [local 사용 시나리오]");
        Console.WriteLine("    - 프로덕션 RT에서 테스트 RT로 전환");
        Console.WriteLine("    - 로깅 RT를 추가하여 감싸기");
        Console.WriteLine("    - 타임아웃이나 취소 토큰 주입");

        // ============================================================
        // 7. Eff 실행 (with Runtime)
        // ============================================================
        MenuHelper.PrintSubHeader("7. Eff 실행 (with Runtime)");

        MenuHelper.PrintExplanation("Runtime이 필요한 Eff는 Runtime을 전달하여 실행합니다.");
        MenuHelper.PrintExplanation("LanguageExt.Sys.Live.Runtime: 실제 시스템 Runtime");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// Live Runtime으로 실행
using LanguageExt.Sys.Live;

var effect = Console<Runtime>.writeLine(""Hello!"");
effect.Run(Runtime.New(), EnvIO.New());

// 또는 ThrowIfFail로 예외 전파
effect.Run(Runtime.New(), EnvIO.New()).ThrowIfFail();");

        // 실제 실행 예시 (콘솔 사용)
        Console.WriteLine();
        Console.WriteLine("    [실제 실행]");

        // LanguageExt.Sys.Live.Runtime 사용
        var liveEffect = LanguageExt.Sys.Console<LanguageExt.Sys.Live.Runtime>
            .writeLine("    > 이것은 Live Runtime으로 출력됩니다!");

        liveEffect.Run(LanguageExt.Sys.Live.Runtime.New(), EnvIO.New());

        // ============================================================
        // 8. 실전 예제: 대화형 프로그램
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전 예제: 대화형 프로그램");

        MenuHelper.PrintExplanation("사용자와 상호작용하는 간단한 프로그램 예시");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 숫자 맞추기 게임 (개념)
static Eff<RT, Unit> numberGuessGame<RT>()
    where RT : Has<Eff<RT>, ConsoleIO>
{
    return from _1   in Console<RT>.writeLine(""1-10 숫자를 맞춰보세요!"")
           from inp  in Console<RT>.readLine
           from num  in parseNumber(inp)
           from _2   in num == 7
                          ? Console<RT>.writeLine(""정답!"")
                          : Console<RT>.writeLine(""틀렸습니다!"")
           select unit;
}");

        // 시뮬레이션 실행
        Console.WriteLine();
        Console.WriteLine("    [시뮬레이션 실행]");
        Console.WriteLine("    > 1-10 숫자를 맞춰보세요!");
        Console.WriteLine("    < 7");
        Console.WriteLine("    > 정답!");

        // Eff<RT, A>의 장점 요약
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("=== Eff<RT, A>의 장점 ===");
        Console.WriteLine("    1. 의존성 명시: 필요한 기능이 타입 시그니처에 표현됨");
        Console.WriteLine("    2. 테스트 용이: Mock Runtime 주입 가능");
        Console.WriteLine("    3. 합성 가능: 여러 효과를 LINQ로 조합");
        Console.WriteLine("    4. 순수성 유지: 효과는 실행 전까지 값으로 존재");

        // 추가 예시: 여러 Trait 조합 실행
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode(@"// 실전 패턴: 시간 측정이 포함된 작업
static Eff<RT, Unit> timedTask<RT>()
    where RT : Has<Eff<RT>, ConsoleIO>, Has<Eff<RT>, TimeIO>
{
    return from start  in Time<RT>.now
           from _1     in Console<RT>.writeLine($""시작: {start}"")
           // ... 작업 수행 ...
           from end    in Time<RT>.now
           from _2     in Console<RT>.writeLine($""완료: {end - start}"")
           select unit;
}");

        MenuHelper.PrintSuccess("Eff 런타임 학습 완료!");
    }
}

// ============================================================
// 부록: 제네릭 Eff 예시 (정적 클래스)
// ============================================================

/// <summary>
/// ConsoleIO trait를 사용하는 예제 효과들
/// </summary>
/// <remarks>
/// 실제 사용 시 이런 방식으로 제네릭 런타임을 요구하는 효과를 정의합니다.
/// </remarks>
public static class GreetingEffects<RT>
    where RT : Has<Eff<RT>, ConsoleIO>
{
    /// <summary>
    /// 간단한 인사 효과
    /// </summary>
    public static Eff<RT, Unit> sayHello =>
        LanguageExt.Sys.Console<RT>.writeLine("Hello from Eff<RT>!");

    /// <summary>
    /// 사용자 이름을 받아 인사하는 효과
    /// </summary>
    public static Eff<RT, string> greetUser =>
        from _1   in LanguageExt.Sys.Console<RT>.writeLine("이름을 입력하세요:")
        from name in LanguageExt.Sys.Console<RT>.readLine
        from _2   in LanguageExt.Sys.Console<RT>.writeLine($"안녕하세요, {name}님!")
        select name;

    /// <summary>
    /// 여러 번 인사하는 효과
    /// </summary>
    public static Eff<RT, Unit> greetMultiple(int count)
    {
        Eff<RT, Unit> result = SuccessEff<RT, Unit>(unit);
        for (int i = 1; i <= count; i++)
        {
            var index = i;
            result = from _ in result
                     from __ in LanguageExt.Sys.Console<RT>.writeLine($"인사 #{index}")
                     select unit;
        }
        return result;
    }
}

/// <summary>
/// ConsoleIO와 TimeIO를 함께 사용하는 예제
/// </summary>
public static class TimedEffects<RT>
    where RT : Has<Eff<RT>, ConsoleIO>, Has<Eff<RT>, TimeIO>
{
    /// <summary>
    /// 현재 시간을 출력
    /// </summary>
    public static Eff<RT, Unit> showTime =>
        from now in Time<RT>.now
        from _   in LanguageExt.Sys.Console<RT>.writeLine($"현재 시간: {now:HH:mm:ss}")
        select unit;

    /// <summary>
    /// 지정된 초만큼 대기 후 메시지 출력
    /// </summary>
    public static Eff<RT, Unit> delayedMessage(int seconds, string message) =>
        from _1 in LanguageExt.Sys.Console<RT>.writeLine($"{seconds}초 후 메시지 출력...")
        from _2 in Time<RT>.sleepFor(TimeSpan.FromSeconds(seconds))
        from _3 in LanguageExt.Sys.Console<RT>.writeLine(message)
        select unit;
}
