using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Sys;
using LanguageExt.Sys.Traits;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;
using static LanguageExt.UnitsOfMeasure;
using System.Text.Json;

namespace LanguageExt101.Part2_Intermediate.Chapter10_Eff;

/// <summary>
/// Eff를 활용한 실전 API 클라이언트 실습입니다.
///
/// 학습 목표:
/// - Eff를 활용한 실전 API 클라이언트 구조
/// - retry, timeout, 폴백 조합
/// - 에러 처리와 복구
/// - 테스트 가능한 구조
///
/// 핵심 개념:
/// 실제 API 클라이언트는 네트워크 오류, 타임아웃, 서버 에러 등
/// 다양한 실패 상황을 처리해야 합니다. Eff와 Schedule을 조합하면
/// 이러한 복원력을 선언적으로 구현할 수 있습니다.
/// </summary>
public static class Exercise07_ApiClient
{
    // 시뮬레이션용 상태
    private static int _requestCount = 0;
    private static readonly Dictionary<int, User> _userCache = new();
    private static readonly Random _random = new();

    public static void Run()
    {
        MenuHelper.PrintHeader("Exercise 07: API 클라이언트");

        // ============================================================
        // 1. API 클라이언트 설계
        // ============================================================
        MenuHelper.PrintSubHeader("1. API 클라이언트 설계");

        MenuHelper.PrintExplanation("trait 기반 구조로 의존성을 추상화합니다.");
        MenuHelper.PrintExplanation("HttpIO trait: HTTP 요청/응답 처리");
        MenuHelper.PrintExplanation("CacheIO trait: 캐시 읽기/쓰기");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// trait 기반 API 클라이언트 구조
public interface HttpIO
{
    Eff<string> Get(string url);
    Eff<string> Post(string url, string body);
}

// 클라이언트는 필요한 trait만 요구
static Eff<RT, User> getUser<RT>(int id)
    where RT : Has<Eff<RT>, HttpIO>, Has<Eff<RT>, ConsoleIO>
{
    return from response in Http<RT>.get($""/api/users/{id}"")
           from user     in parseJson<User>(response)
           select user;
}");

        // 시뮬레이션 설명
        Console.WriteLine();
        Console.WriteLine("    [이 실습에서는 실제 HTTP 대신 시뮬레이션을 사용합니다]");

        // ============================================================
        // 2. 기본 요청
        // ============================================================
        MenuHelper.PrintSubHeader("2. 기본 요청");

        MenuHelper.PrintExplanation("GET/POST 요청의 기본 구현");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// GET 요청 예시
var response = await httpClient.GetAsync(url);
var content = await response.Content.ReadAsStringAsync();

// Eff로 감싸기
static Eff<string> get(string url) =>
    liftEff(async () => await httpClient.GetStringAsync(url));");

        // 시뮬레이션 실행
        _requestCount = 0;
        var getResult = SimulateGet("/api/users/1").Run();
        MenuHelper.PrintResult("GET /api/users/1", getResult);

        // POST 예시
        var postResult = SimulatePost("/api/users", """{"name":"홍길동"}""").Run();
        MenuHelper.PrintResult("POST /api/users", postResult);

        // ============================================================
        // 3. 재시도 적용
        // ============================================================
        MenuHelper.PrintSubHeader("3. 재시도 적용");

        MenuHelper.PrintExplanation("Schedule을 사용한 재시도 정책");
        MenuHelper.PrintExplanation("exponential: 지수 백오프");
        MenuHelper.PrintExplanation("recurs: 최대 재시도 횟수");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 재시도 패턴
static Eff<RT, string> getWithRetry<RT>(string url)
    where RT : Has<Eff<RT>, HttpIO>
{
    return retry(
        Schedule.exponential(100 * ms) | Schedule.recurs(3),
        Http<RT>.get(url));
}");

        // 시뮬레이션: 2번 실패 후 성공
        Console.WriteLine();
        Console.WriteLine("    [재시도 시뮬레이션]");
        _requestCount = 0;
        var retryResult = RetrySimulation(
            () => SimulateUnstableGet("/api/data", failUntil: 3),
            maxRetries: 5,
            delayMs: 100
        ).Run();
        MenuHelper.PrintResult("재시도 결과", retryResult);

        // ============================================================
        // 4. 타임아웃
        // ============================================================
        MenuHelper.PrintSubHeader("4. 타임아웃");

        MenuHelper.PrintExplanation("timeout: 지정 시간 초과 시 실패");
        MenuHelper.PrintExplanation("Errors.TimedOut으로 타임아웃 감지");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 타임아웃 패턴
static Eff<RT, string> getWithTimeout<RT>(string url)
    where RT : Has<Eff<RT>, HttpIO>, Has<Eff<RT>, TimeIO>
{
    return timeout(5 * seconds, Http<RT>.get(url));
}

// 타임아웃 시 에러 처리
from response in getWithTimeout<RT>(url)
               | @catch(Errors.TimedOut, SuccessEff(""타임아웃 발생""))
select response;");

        // 시뮬레이션
        Console.WriteLine();
        Console.WriteLine("    [타임아웃 시뮬레이션]");

        var fastResult = SimulateWithTimeout("/api/fast", 50, timeoutMs: 200).Run();
        MenuHelper.PrintResult("빠른 요청 (50ms)", fastResult);

        var slowResult = SimulateWithTimeout("/api/slow", 300, timeoutMs: 200).Run();
        MenuHelper.PrintResult("느린 요청 (300ms, 타임아웃)", slowResult);

        // ============================================================
        // 5. 폴백 체인
        // ============================================================
        MenuHelper.PrintSubHeader("5. 폴백 체인");

        MenuHelper.PrintExplanation("주 요청 실패 시 대체 소스 사용");
        MenuHelper.PrintExplanation("@catch 체이닝으로 다중 폴백 구현");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 폴백 체인
static Eff<RT, User> getUserWithFallback<RT>(int id)
    where RT : Has<Eff<RT>, HttpIO>, Has<Eff<RT>, CacheIO>
{
    return getFromApi<RT>(id)
         | @catch(Errors.TimedOut, getFromCache<RT>(id))
         | @catch(e => e.Code == 503, getFromBackupServer<RT>(id))
         | @catch(e => SuccessEff(User.Guest));  // 최종 폴백
}");

        // 시뮬레이션
        Console.WriteLine();
        Console.WriteLine("    [폴백 체인 시뮬레이션]");

        var fallbackResult = SimulateFallbackChain(userId: 1).Run();
        fallbackResult.Match(
            Succ: v => Console.WriteLine($"    결과: {v}"),
            Fail: e => Console.WriteLine($"    실패: {e.Message}")
        );

        // ============================================================
        // 6. 응답 파싱
        // ============================================================
        MenuHelper.PrintSubHeader("6. 응답 파싱");

        MenuHelper.PrintExplanation("JSON 응답을 도메인 객체로 변환");
        MenuHelper.PrintExplanation("파싱 실패도 Eff 에러로 처리");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// JSON 파싱
static Eff<T> parseJson<T>(string json) =>
    liftEff(() => JsonSerializer.Deserialize<T>(json)
        ?? throw new Exception(""파싱 결과가 null입니다""));

// 사용
from response in Http<RT>.get(url)
from user     in parseJson<User>(response)
select user;");

        // 파싱 예시
        Console.WriteLine();
        var validJson = """{"Id":1,"Name":"홍길동","Email":"hong@example.com"}""";
        var invalidJson = """{"invalid json""";

        var validParse = ParseJson<User>(validJson).Run();
        MenuHelper.PrintResult("유효한 JSON", validParse);

        var invalidParse = ParseJson<User>(invalidJson).Run();
        MenuHelper.PrintResult("유효하지 않은 JSON", invalidParse);

        // ============================================================
        // 7. 캐싱 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("7. 캐싱 패턴");

        MenuHelper.PrintExplanation("캐시 우선 조회 (Cache-First)");
        MenuHelper.PrintExplanation("캐시 미스 시 API 호출 후 캐시 저장");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 캐시 패턴
static Eff<RT, User> getUserWithCache<RT>(int id)
    where RT : Has<Eff<RT>, HttpIO>, Has<Eff<RT>, CacheIO>
{
    return tryGetFromCache<RT>(id)
         | @catch(e => e.Code == CacheError.NotFound,
                  from user in fetchFromApi<RT>(id)
                  from _    in saveToCache<RT>(id, user)
                  select user);
}");

        // 시뮬레이션
        Console.WriteLine();
        Console.WriteLine("    [캐시 시뮬레이션]");
        _userCache.Clear();

        // 첫 번째 요청: 캐시 미스 -> API 호출
        var user1Result = GetUserWithCache(1).Run();
        user1Result.Match(
            Succ: v => Console.WriteLine($"    첫 번째 요청 (캐시 미스): {v}"),
            Fail: e => Console.WriteLine($"    실패: {e.Message}")
        );

        // 두 번째 요청: 캐시 히트
        var user1Cached = GetUserWithCache(1).Run();
        user1Cached.Match(
            Succ: v => Console.WriteLine($"    두 번째 요청 (캐시 히트): {v} [from cache]"),
            Fail: e => Console.WriteLine($"    실패: {e.Message}")
        );

        // ============================================================
        // 8. 통합 예제: 복원력 있는 API 클라이언트
        // ============================================================
        MenuHelper.PrintSubHeader("8. 통합 예제: 복원력 있는 API 클라이언트");

        MenuHelper.PrintExplanation("timeout + retry + fallback + cache 조합");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 완전한 복원력 있는 클라이언트
static Eff<RT, User> getResilientUser<RT>(int id)
    where RT : Has<Eff<RT>, HttpIO>,
               Has<Eff<RT>, CacheIO>,
               Has<Eff<RT>, TimeIO>,
               Has<Eff<RT>, ConsoleIO>
{
    return timeout(5 * seconds,
             retry(Schedule.exponential(100 * ms) | Schedule.recurs(3),
                   from _    in Console<RT>.writeLine($""사용자 {id} 조회 중..."")
                   from resp in Http<RT>.get($""/api/users/{id}"")
                   from user in parseJson<User>(resp)
                   from __   in guard(user.IsActive, Error.New(""비활성 사용자""))
                   select user))
         | @catch(Errors.TimedOut, getFromCache<RT>(id))
         | @catch(e => e.Code == 404,
                  from _ in Console<RT>.writeLine(""사용자를 찾을 수 없음"")
                  select User.Guest)
         | @catch(logAndReturnDefault<RT>);
}");

        // 통합 테스트 시뮬레이션
        Console.WriteLine();
        Console.WriteLine("    [통합 테스트 시뮬레이션]");
        Console.WriteLine();

        // 시나리오 1: 정상 조회
        Console.WriteLine("    === 시나리오 1: 정상 조회 ===");
        _requestCount = 0;
        var scenario1 = ResilientGetUser(1, scenario: "success").Run();
        scenario1.Match(
            Succ: v => MenuHelper.PrintResult("결과", v),
            Fail: e => MenuHelper.PrintResult("실패", e.Message)
        );

        // 시나리오 2: 일시적 실패 후 성공
        Console.WriteLine();
        Console.WriteLine("    === 시나리오 2: 재시도 후 성공 ===");
        _requestCount = 0;
        var scenario2 = ResilientGetUser(2, scenario: "retry").Run();
        scenario2.Match(
            Succ: v => MenuHelper.PrintResult("결과", v),
            Fail: e => MenuHelper.PrintResult("실패", e.Message)
        );

        // 시나리오 3: 타임아웃 -> 캐시 폴백
        Console.WriteLine();
        Console.WriteLine("    === 시나리오 3: 타임아웃 -> 캐시 ===");
        _userCache[3] = new User(3, "캐시된사용자", "cached@example.com");
        var scenario3 = ResilientGetUser(3, scenario: "timeout").Run();
        scenario3.Match(
            Succ: v => MenuHelper.PrintResult("결과 (캐시)", v),
            Fail: e => MenuHelper.PrintResult("실패", e.Message)
        );

        // 시나리오 4: 사용자 없음 -> 기본값
        Console.WriteLine();
        Console.WriteLine("    === 시나리오 4: 사용자 없음 ===");
        var scenario4 = ResilientGetUser(999, scenario: "not_found").Run();
        scenario4.Match(
            Succ: v => MenuHelper.PrintResult("결과 (기본값)", v),
            Fail: e => MenuHelper.PrintResult("실패", e.Message)
        );

        // 요약
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("=== API 클라이언트 패턴 요약 ===");
        Console.WriteLine("    1. trait 기반 설계: 의존성 추상화, 테스트 용이");
        Console.WriteLine("    2. retry: Schedule로 재시도 정책 선언");
        Console.WriteLine("    3. timeout: 시간 제한으로 무한 대기 방지");
        Console.WriteLine("    4. @catch 체인: 계층적 에러 처리");
        Console.WriteLine("    5. 캐시: 성능 향상과 폴백 소스");
        Console.WriteLine("    6. guard: 비즈니스 규칙 검증");

        MenuHelper.PrintSuccess("실습 07 완료!");
    }

    // ============================================================
    // 헬퍼 함수들 (시뮬레이션)
    // ============================================================

    private static Eff<string> SimulateGet(string url) =>
        liftEff(() =>
        {
            _requestCount++;
            Console.WriteLine($"    [GET] {url} (요청 #{_requestCount})");
            Thread.Sleep(50);
            return $"{{\"status\":\"ok\",\"url\":\"{url}\"}}";
        });

    private static Eff<string> SimulatePost(string url, string body) =>
        liftEff(() =>
        {
            _requestCount++;
            Console.WriteLine($"    [POST] {url} (요청 #{_requestCount})");
            Console.WriteLine($"    Body: {body}");
            Thread.Sleep(50);
            return """{"status":"created","id":123}""";
        });

    private static Eff<string> SimulateUnstableGet(string url, int failUntil) =>
        liftEff(() =>
        {
            _requestCount++;
            Console.WriteLine($"    [GET] {url} (시도 #{_requestCount})");
            if (_requestCount < failUntil)
            {
                Console.WriteLine($"    -> 실패: 일시적 오류");
                throw new Exception("일시적 오류");
            }
            return $"{{\"data\":\"success after {_requestCount} attempts\"}}";
        });

    private static IO<Fin<string>> RetrySimulation(Func<Eff<string>> operation, int maxRetries, int delayMs) =>
        IO.lift<Fin<string>>(() =>
        {
            Fin<string> lastResult = Fin.Fail<string>(Error.New("초기 상태"));
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                var result = operation().Run();
                if (result.IsSucc)
                    return result;

                lastResult = result;
                Console.WriteLine($"    재시도 대기 {delayMs}ms...");
                Thread.Sleep(delayMs);
                delayMs *= 2; // 지수 백오프
            }
            return Fin.Fail<string>(Error.New("최대 재시도 초과"));
        });

    private static IO<Fin<string>> SimulateWithTimeout(string url, int responseTimeMs, int timeoutMs) =>
        IO.lift<Fin<string>>(() =>
        {
            Console.WriteLine($"    [요청] {url} (응답 예상: {responseTimeMs}ms, 타임아웃: {timeoutMs}ms)");

            if (responseTimeMs > timeoutMs)
            {
                Thread.Sleep(timeoutMs);
                return Fin.Fail<string>(Errors.TimedOut);
            }

            Thread.Sleep(responseTimeMs);
            return Fin.Succ($"{{\"url\":\"{url}\"}}");
        });

    private static IO<Fin<string>> SimulateFallbackChain(int userId) =>
        IO.lift<Fin<string>>(() =>
        {
            // 1. 주 API 시도 (실패)
            Console.WriteLine("    [1] 주 API 호출...");
            Console.WriteLine("    -> 실패: 서버 응답 없음");

            // 2. 백업 서버 시도 (실패)
            Console.WriteLine("    [2] 백업 서버 호출...");
            Console.WriteLine("    -> 실패: 연결 거부");

            // 3. 캐시 조회 (성공)
            Console.WriteLine("    [3] 캐시 조회...");
            return Fin.Succ($"{{\"id\":{userId},\"name\":\"캐시된사용자\",\"cached\":true}}");
        });

    private static Eff<T> ParseJson<T>(string json) =>
        liftEff(() => JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new Exception("파싱 결과가 null입니다"));

    private static Eff<User> GetUserWithCache(int id) =>
        liftEff(() =>
        {
            // 캐시 확인
            if (_userCache.TryGetValue(id, out var cached))
            {
                return cached;
            }

            // API 호출 시뮬레이션
            Console.WriteLine($"    API 호출: /api/users/{id}");
            var user = new User(id, $"사용자{id}", $"user{id}@example.com");

            // 캐시 저장
            _userCache[id] = user;
            return user;
        });

    private static IO<Fin<User>> ResilientGetUser(int id, string scenario) =>
        IO.lift<Fin<User>>(() =>
        {
            switch (scenario)
            {
                case "success":
                    Console.WriteLine($"    요청: GET /api/users/{id}");
                    return Fin.Succ(new User(id, $"사용자{id}", $"user{id}@example.com"));

                case "retry":
                    _requestCount++;
                    Console.WriteLine($"    요청: GET /api/users/{id} (시도 #{_requestCount})");
                    if (_requestCount < 3)
                    {
                        Console.WriteLine("    -> 503 Service Unavailable, 재시도...");
                        Thread.Sleep(100);
                        return ResilientGetUser(id, scenario).Run();
                    }
                    return Fin.Succ(new User(id, $"사용자{id}", $"user{id}@example.com"));

                case "timeout":
                    Console.WriteLine($"    요청: GET /api/users/{id}");
                    Console.WriteLine("    -> 타임아웃! 캐시 확인...");
                    if (_userCache.TryGetValue(id, out var cached))
                        return Fin.Succ(cached);
                    return Fin.Fail<User>(Errors.TimedOut);

                case "not_found":
                    Console.WriteLine($"    요청: GET /api/users/{id}");
                    Console.WriteLine("    -> 404 Not Found, 기본값 사용...");
                    return Fin.Succ(User.Guest);

                default:
                    return Fin.Fail<User>(Error.New("알 수 없는 시나리오"));
            }
        });
}

// ============================================================
// 도메인 모델
// ============================================================

/// <summary>
/// 사용자 도메인 모델
/// </summary>
public record User(int Id, string Name, string Email)
{
    /// <summary>
    /// 활성 사용자 여부
    /// </summary>
    public bool IsActive => Id > 0;

    /// <summary>
    /// 게스트 사용자 (기본값)
    /// </summary>
    public static User Guest => new(0, "Guest", "guest@example.com");

    public override string ToString() => $"User({Id}, {Name}, {Email})";
}

// ============================================================
// 부록: trait 기반 API 클라이언트 구조 (개념 코드)
// ============================================================

/// <summary>
/// HTTP 요청 trait (개념)
/// </summary>
/// <remarks>
/// 실제 구현에서는 LanguageExt.Sys의 HttpIO를 사용하거나
/// 커스텀 trait을 정의합니다.
/// </remarks>
public interface IHttpIO
{
    Eff<string> Get(string url);
    Eff<string> Post(string url, string body);
}

/// <summary>
/// 캐시 trait (개념)
/// </summary>
public interface ICacheIO<T>
{
    Eff<Option<T>> TryGet(string key);
    Eff<Unit> Set(string key, T value, TimeSpan? ttl = null);
    Eff<Unit> Remove(string key);
}

/// <summary>
/// 복원력 있는 API 클라이언트 인터페이스 (개념)
/// </summary>
public static class ResilientApiClient<RT>
    where RT : Has<Eff<RT>, ConsoleIO>, Has<Eff<RT>, TimeIO>
{
    /// <summary>
    /// 복원력 있는 사용자 조회 (개념 코드)
    /// </summary>
    public static Eff<RT, User> GetUser(int id) =>
        timeout(5 * seconds,
            retry(Schedule.exponential(100 * ms) | Schedule.recurs(3),
                  from _ in LanguageExt.Sys.Console<RT>.writeLine($"사용자 {id} 조회 중...")
                  from user in SimulateFetch(id)
                  from __ in user.IsActive ? unitEff : FailEff<RT, Unit>(Error.New("비활성 사용자"))
                  select user))
      | @catch(Errors.TimedOut, GetFromCache(id))
      | @catch(e => e.Code == 404, SuccessEff<RT, User>(User.Guest))
      | @catch(e => LogAndDefault(e));

    private static Eff<RT, User> SimulateFetch(int id) =>
        SuccessEff<RT, User>(new User(id, $"사용자{id}", $"user{id}@example.com"));

    private static Eff<RT, User> GetFromCache(int id) =>
        SuccessEff<RT, User>(new User(id, "캐시사용자", "cached@example.com"));

    private static Eff<RT, User> LogAndDefault(Error e) =>
        from _ in LanguageExt.Sys.Console<RT>.writeLine($"에러 발생: {e.Message}")
        select User.Guest;
}
