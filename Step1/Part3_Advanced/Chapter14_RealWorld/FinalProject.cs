using LanguageExt;
using LanguageExt.Traits;
using LanguageExt.UnsafeValueAccess;
using LanguageExt101.Common;
using static LanguageExt.Prelude;
using Console = System.Console;

namespace LanguageExt101.Part3_Advanced.Chapter14_RealWorld;

/// <summary>
/// 최종 프로젝트: Pontoon(21) 카드 게임
///
/// 이 예제는 LanguageExt 5.0의 고급 기능들을 활용한 실무 수준의 함수형 프로그래밍 예제입니다.
/// Monad Transformer 스택을 사용하여 상태 관리, 옵션 처리, IO 부수효과를 우아하게 합성합니다.
///
/// ========================================================================
/// Pontoon(폰툰) 게임 규칙
/// ========================================================================
///
/// Pontoon은 Blackjack(블랙잭)과 유사한 영국식 카드 게임입니다.
/// "21" 또는 "Vingt-Un(벵퉁)"이라고도 불립니다.
///
/// [게임 목표]
/// - 카드의 합이 21에 최대한 가깝게, 하지만 21을 넘지 않도록 하는 것
/// - 21을 초과하면 "Bust(버스트)"되어 패배
///
/// [카드 점수]
/// - 숫자 카드 (2-10): 숫자 그대로의 점수
/// - 그림 카드 (Jack, Queen, King): 각각 10점
/// - Ace: 1점 또는 11점 (유리한 쪽으로 자동 계산)
///
/// [게임 진행]
/// 1. 플레이어 등록: 참가자 이름 입력 (빈 입력 시 등록 완료)
/// 2. 초기 배분: 각 플레이어에게 카드 2장씩 배분
/// 3. 턴 진행: 각 플레이어가 순서대로 선택
///    - Stick(스틱): 더 이상 카드를 받지 않고 현재 점수로 확정
///    - Twist(트위스트): 카드 1장을 더 받음
/// 4. 라운드 종료 조건:
///    - 모든 플레이어가 Stick 또는 Bust
///    - 누군가 21점 달성
/// 5. 승자 결정: Bust 되지 않고 21에 가장 가까운 점수를 가진 플레이어
///
/// [용어]
/// - Pontoon: Ace + 10점 카드로 21을 만드는 것 (가장 좋은 패)
/// - Bust: 21점 초과 (패배)
/// - Stick: 카드를 더 받지 않음
/// - Twist: 카드를 한 장 더 받음
/// - Five Card Trick: 5장의 카드로 21 이하를 만드는 것 (Pontoon 다음으로 높은 패)
///
/// ========================================================================
/// 기술적 학습 포인트
/// ========================================================================
///
/// 1. Monad Transformer 스택
///    Game(A) = StateT(GameState, OptionT(IO), A)
///    - IO: 콘솔 입출력 등 부수효과
///    - OptionT: 게임 취소/종료 (None = 게임 끝)
///    - StateT: 게임 상태 관리 (덱, 플레이어)
///
/// 2. 불변 데이터 구조
///    - record 타입으로 도메인 모델링
///    - HashMap으로 플레이어 상태 관리
///    - Seq으로 카드 리스트 관리
///
/// 3. Reader-like 패턴
///    - Player.with()로 현재 플레이어 컨텍스트 관리
///    - 플레이어별 로직을 깔끔하게 분리
///
/// 4. LINQ 쿼리 구문
///    - from/select로 게임 로직을 선언적으로 표현
///    - 복잡한 효과 합성을 읽기 쉽게 표현
/// </summary>
public static class FinalProject
{
    public static void Run()
    {
        MenuHelper.PrintHeader("최종 프로젝트: Pontoon(21) 카드 게임");

        // ================================================================
        // 개념 설명 출력
        // ================================================================
        MenuHelper.PrintExplanation("Monad Transformer 스택을 활용한 함수형 카드 게임 구현");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintSubHeader("핵심 개념: Monad Transformer 스택");
        MenuHelper.PrintCode("Game<A> = StateT<GameState, OptionT<IO>, A>");
        MenuHelper.PrintExplanation("세 가지 효과를 하나의 모나드로 합성:");
        MenuHelper.PrintResult("  - IO", "콘솔 입출력, 랜덤 셔플 등 부수효과");
        MenuHelper.PrintResult("  - OptionT", "게임 취소/종료 (None = 게임 끝)");
        MenuHelper.PrintResult("  - StateT", "게임 상태 (덱, 플레이어) 관리");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintSubHeader("불변 데이터 모델");
        MenuHelper.PrintCode("record Card(int Index)");
        MenuHelper.PrintCode("record Deck(Seq<Card> Cards)");
        MenuHelper.PrintCode("record PlayerState(Seq<Card> Cards, bool StickState)");
        MenuHelper.PrintCode("record GameState(HashMap<Player, PlayerState> State, Deck Deck, Option<Player> CurrentPlayer)");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintSubHeader("Reader-like 패턴");
        MenuHelper.PrintCode("Player.with(player, computation)  // 플레이어 컨텍스트에서 연산 실행");
        MenuHelper.PrintCode("Players.with(players, computation)  // 여러 플레이어 순회");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintSubHeader("게임 규칙 요약");
        MenuHelper.PrintResult("목표", "카드 합이 21에 가장 가깝게 (21 초과 시 Bust)");
        MenuHelper.PrintResult("Ace", "1점 또는 11점 (유리한 쪽으로 계산)");
        MenuHelper.PrintResult("그림 카드", "Jack, Queen, King = 각 10점");
        MenuHelper.PrintResult("Stick(S)", "현재 점수로 확정");
        MenuHelper.PrintResult("Twist(T)", "카드 1장 추가");
        MenuHelper.PrintBlankLines();

        Console.WriteLine("  게임을 시작합니다...");
        Console.WriteLine();

        // ================================================================
        // 게임 실행
        // ================================================================
        PontoonGame.play
            .Run(PontoonGameState.Zero)  // StateT 실행: 초기 상태로 시작
            .Run()                       // IO 실행
            .Ignore();                   // 결과 무시

        MenuHelper.PrintBlankLines();
        MenuHelper.PrintSuccess("게임 종료!");
    }
}

// ============================================================================
// 도메인 모델
// ============================================================================

#region Domain Models

/// <summary>
/// 카드를 나타내는 불변 레코드
///
/// Index 0-51로 52장의 카드를 표현:
/// - 0-12: 하트 (Ace, 2-10, Jack, Queen, King)
/// - 13-25: 클럽
/// - 26-38: 스페이드
/// - 39-51: 다이아몬드
///
/// FaceValues: Ace는 [1, 11] 두 가지 값을 가질 수 있음
/// </summary>
public record PontoonCard(int Index)
{
    /// <summary>카드 이름 (Ace, 2-10, Jack, Queen, King)</summary>
    public string Name =>
        (Index % 13) switch
        {
            0 => "Ace",
            10 => "Jack",
            11 => "Queen",
            12 => "King",
            var ix => $"{ix + 1}"
        };

    /// <summary>카드 무늬 (Hearts, Clubs, Spades, Diamonds)</summary>
    public string Suit =>
        Index switch
        {
            < 13 => "Hearts",
            < 26 => "Clubs",
            < 39 => "Spades",
            < 52 => "Diamonds",
            _ => throw new NotSupportedException()
        };

    public override string ToString() =>
        $"{Name} of {Suit}";

    /// <summary>
    /// 카드의 가능한 점수값들
    /// Ace는 1 또는 11, 그림 카드는 10, 나머지는 숫자 그대로
    /// </summary>
    public Seq<int> FaceValues =>
        (Index % 13) switch
        {
            0 => [1, 11],    // Ace: 1점 또는 11점
            10 => [10],      // Jack
            11 => [10],      // Queen
            12 => [10],      // King
            var x => [x + 1] // 2-10
        };
}

/// <summary>
/// 덱(카드 더미)을 나타내는 불변 레코드
///
/// 함수형 방식으로 카드를 관리:
/// - 셔플은 새로운 Deck 인스턴스 생성
/// - 딜은 Head(맨 위 카드)와 Tail(나머지) 사용
/// </summary>
public record PontoonDeck(Seq<PontoonCard> Cards)
{
    public static PontoonDeck Empty = new([]);

    public override string ToString() =>
        Cards.ToFullArrayString();
}

/// <summary>
/// 플레이어를 나타내는 불변 레코드
/// HashMap의 키로 사용됨
/// </summary>
public record PontoonPlayer(string Name)
{
    public override string ToString() =>
        Name;
}

/// <summary>
/// 개별 플레이어의 상태를 나타내는 불변 레코드
///
/// - Cards: 플레이어가 가진 카드들
/// - StickState: Stick 선택 여부
/// - Scores: 가능한 모든 점수 조합 (Ace 때문에 여러 개 가능)
/// </summary>
public record PontoonPlayerState(Seq<PontoonCard> Cards, bool StickState)
{
    public static readonly PontoonPlayerState Zero = new([], false);

    /// <summary>카드 추가 (불변: 새 인스턴스 반환)</summary>
    public PontoonPlayerState AddCard(PontoonCard card) =>
        this with { Cards = Cards.Add(card) };

    /// <summary>Stick 상태로 변경</summary>
    public PontoonPlayerState Stick() =>
        this with { StickState = true };

    /// <summary>
    /// 가능한 모든 점수 계산
    ///
    /// Ace가 여러 장일 때 모든 조합을 계산합니다.
    /// 예: Ace 2장 → [2, 12, 22] (1+1, 1+11, 11+11)
    ///
    /// 이 로직은 카르테시안 곱(Cartesian product)을 사용합니다:
    /// 각 카드의 가능한 값들을 LINQ로 조합
    /// </summary>
    public Seq<int> Scores =>
        Cards.Map(c => c.FaceValues)
             .Fold(Seq(Seq<int>()),
                   (s, vs) =>
                       from x in s
                       from v in vs
                       select x.Add(v))
             .Map(s => s.Sum<Seq, int>())
             .Distinct()
             .OrderBy(s => s)
             .AsIterable()
             .ToSeq();

    /// <summary>아직 게임 진행 중인지 (21 미달, Stick 안 함, Bust 아님)</summary>
    public bool StillInTheGame() =>
        !Scores.Exists(s => s == 21) &&
        !StickState &&
        !IsBust;

    /// <summary>Bust 되지 않은 최고 점수</summary>
    public Option<int> MaximumNonBustScore =>
        Scores.Filter(s => s <= 21).Last;

    /// <summary>21점 달성 여부</summary>
    public bool Has21 =>
        Scores.Exists(s => s == 21);

    /// <summary>Bust 여부 (모든 가능 점수가 21 초과)</summary>
    public bool IsBust =>
        Scores.ForAll(s => s > 21);
}

/// <summary>
/// 전체 게임 상태를 나타내는 불변 레코드
///
/// StateT 모나드 트랜스포머가 관리하는 상태입니다.
/// - State: 플레이어별 상태 (HashMap으로 O(log n) 접근)
/// - Deck: 남은 카드 덱
/// - CurrentPlayer: 현재 턴인 플레이어 (Option으로 없을 수 있음)
/// </summary>
public record PontoonGameState(
    HashMap<PontoonPlayer, PontoonPlayerState> State,
    PontoonDeck Deck,
    Option<PontoonPlayer> CurrentPlayer)
{
    public static readonly PontoonGameState Zero = new([], PontoonDeck.Empty, None);
}

#endregion

// ============================================================================
// Game 모나드 (Monad Transformer 스택)
// ============================================================================

#region Game Monad

/// <summary>
/// Game 모나드 - 게임 로직을 위한 커스텀 모나드 트랜스포머 스택
///
/// 타입 구조:
/// Game(A) = StateT(PontoonGameState, OptionT(IO), A)
///
/// 이 스택의 의미:
/// 1. IO: 가장 바깥쪽, 콘솔 입출력/랜덤 등 부수효과
/// 2. OptionT: IO 위에 쌓임, None으로 게임 취소/종료 표현
/// 3. StateT: 가장 안쪽, 게임 상태 관리
///
/// 실행 순서 (안→밖):
/// game.runGame.Run(state).As().Run().As()
///      StateT     OptionT      IO
/// </summary>
public record PontoonGame<A>(StateT<PontoonGameState, OptionT<IO>, A> runGame) : K<PontoonGame, A>
{
    /// <summary>순수 값을 Game 모나드로 리프트</summary>
    public static PontoonGame<A> Pure(A x) =>
        new(StateT<PontoonGameState, OptionT<IO>, A>.Pure(x));

    /// <summary>게임 취소 (OptionT의 None)</summary>
    public static PontoonGame<A> None =>
        new(StateT<PontoonGameState, OptionT<IO>, A>.Lift(OptionT.None<IO, A>()));

    /// <summary>Option을 Game으로 리프트 (None이면 게임 취소)</summary>
    public static PontoonGame<A> Lift(Option<A> mx) =>
        new(StateT<PontoonGameState, OptionT<IO>, A>.Lift(OptionT.lift<IO, A>(mx)));

    /// <summary>IO를 Game으로 리프트</summary>
    public static PontoonGame<A> LiftIO(IO<A> mx) =>
        new(StateT<PontoonGameState, OptionT<IO>, A>.LiftIO(mx));

    // Functor 연산
    public PontoonGame<B> Map<B>(Func<A, B> f) =>
        this.Kind().Map(f).As();

    public PontoonGame<B> Select<B>(Func<A, B> f) =>
        this.Kind().Map(f).As();

    // Monad 연산
    public PontoonGame<B> Bind<B>(Func<A, K<PontoonGame, B>> f) =>
        this.Kind().Bind(f).As();

    public PontoonGame<B> Bind<B>(Func<A, IO<B>> f) =>
        this.Kind().Bind(f).As();

    // LINQ 지원
    public PontoonGame<C> SelectMany<B, C>(Func<A, K<PontoonGame, B>> bind, Func<A, B, C> project) =>
        Bind(a => bind(a).Map(b => project(a, b)));

    public PontoonGame<C> SelectMany<B, C>(Func<A, IO<B>> bind, Func<A, B, C> project) =>
        SelectMany(a => MonadIO.liftIO<PontoonGame, B>(bind(a)), project);

    public PontoonGame<C> SelectMany<B, C>(Func<A, K<IO, B>> bind, Func<A, B, C> project) =>
        SelectMany(a => MonadIO.liftIO<PontoonGame, B>(bind(a).As()), project);

    public PontoonGame<C> SelectMany<B, C>(Func<A, Pure<B>> bind, Func<A, B, C> project) =>
        Map(a => project(a, bind(a).Value));

    // 암시적 변환
    public static implicit operator PontoonGame<A>(Pure<A> ma) =>
        Pure(ma.Value);

    public static implicit operator PontoonGame<A>(IO<A> ma) =>
        PontoonGame.liftIO(ma);

    public static implicit operator PontoonGame<A>(Option<A> ma) =>
        PontoonGame.lift(ma);
}

/// <summary>
/// Game 모나드 타입클래스 인스턴스
///
/// Deriving을 사용하여 Monad, Stateful 인스턴스를 자동 생성합니다.
/// MonadIO는 IO 연산을 Game으로 리프트할 수 있게 합니다.
/// </summary>
public partial class PontoonGame :
    Deriving.Monad<PontoonGame, StateT<PontoonGameState, OptionT<IO>>>,
    Deriving.Stateful<PontoonGame, StateT<PontoonGameState, OptionT<IO>>, PontoonGameState>,
    MonadIO<PontoonGame>
{
    /// <summary>Game을 기반 트랜스포머로 변환 (Deriving 필수)</summary>
    public static K<StateT<PontoonGameState, OptionT<IO>>, A> Transform<A>(K<PontoonGame, A> fa) =>
        fa.As().runGame;

    /// <summary>기반 트랜스포머를 Game으로 변환 (Deriving 필수)</summary>
    public static K<PontoonGame, A> CoTransform<A>(K<StateT<PontoonGameState, OptionT<IO>>, A> fa) =>
        new PontoonGame<A>(fa.As());

    /// <summary>IO를 Game으로 리프트 (MonadIO 구현)</summary>
    public static K<PontoonGame, A> LiftIO<A>(IO<A> ma) =>
        new PontoonGame<A>(StateT.liftIO<PontoonGameState, OptionT<IO>, A>(ma));
}

#endregion

// ============================================================================
// Game 모나드 확장 메서드
// ============================================================================

#region Game Extensions

/// <summary>
/// K(PontoonGame, A) 확장 메서드
/// </summary>
public static class PontoonGameExtensions
{
    /// <summary>K(PontoonGame, A)를 PontoonGame(A)로 변환</summary>
    public static PontoonGame<A> As<A>(this K<PontoonGame, A> ma) =>
        (PontoonGame<A>)ma;

    /// <summary>
    /// Game 트랜스포머를 실행하여 IO로 변환
    ///
    /// 실행 순서:
    /// 1. StateT.Run(state) → OptionT(IO, (A, State))
    /// 2. OptionT.Run() → IO(Option((A, State)))
    /// </summary>
    public static IO<Option<(A Value, PontoonGameState State)>> Run<A>(
        this K<PontoonGame, A> ma, PontoonGameState state) =>
        ma.As().runGame.Run(state).As().Run().As();

    // LINQ SelectMany 확장
    public static PontoonGame<C> SelectMany<A, B, C>(
        this K<PontoonGame, A> ma,
        Func<A, K<PontoonGame, B>> bind,
        Func<A, B, C> project) =>
        ma.As().SelectMany(bind, project);

    public static PontoonGame<C> SelectMany<A, B, C>(
        this K<IO, A> ma,
        Func<A, K<PontoonGame, B>> bind,
        Func<A, B, C> project) =>
        MonadIO.liftIO<PontoonGame, A>(ma.As()).SelectMany(bind, project);

    public static PontoonGame<C> SelectMany<A, B, C>(
        this IO<A> ma,
        Func<A, K<PontoonGame, B>> bind,
        Func<A, B, C> project) =>
        MonadIO.liftIO<PontoonGame, A>(ma).SelectMany(bind, project);

    public static PontoonGame<C> SelectMany<A, B, C>(
        this Pure<A> ma,
        Func<A, K<PontoonGame, B>> bind,
        Func<A, B, C> project) =>
        PontoonGame.Pure(ma.Value).SelectMany(bind, project);
}

#endregion

// ============================================================================
// Game 모듈 함수
// ============================================================================

#region Game Module

/// <summary>
/// Game 모나드의 모듈 함수들
///
/// 상태 접근, 수정, 리프팅 등 핵심 연산을 제공합니다.
/// </summary>
public partial class PontoonGame
{
    /// <summary>순수 값을 Game으로 리프트</summary>
    public static PontoonGame<A> Pure<A>(A value) =>
        PontoonGame<A>.Pure(value);

    /// <summary>unit을 반환하는 캐시된 Game</summary>
    public static readonly PontoonGame<Unit> unitM =
        PontoonGame<Unit>.Pure(unit);

    /// <summary>
    /// 게임 취소
    /// OptionT의 None을 사용하여 게임을 종료합니다.
    /// 모든 후속 연산이 자동으로 스킵됩니다.
    /// </summary>
    public static readonly PontoonGame<Unit> cancel =
        lift(Option<Unit>.None);

    /// <summary>현재 게임 상태 전체를 읽기</summary>
    public static PontoonGame<PontoonGameState> state =>
        new(StateT.get<OptionT<IO>, PontoonGameState>());

    /// <summary>게임이 아직 진행 중인지 확인</summary>
    public static PontoonGame<bool> isGameActive =>
        from non in nonActivePlayersState
        from res in non.Exists(p => p.State.Has21 || p.State.StickState)
                        ? activePlayersState.Map(ps => ps.Count > 0)
                        : activePlayersState.Map(ps => ps.Count > 1)
        select res;

    /// <summary>현재 최고 점수 (Bust 아닌 플레이어 중)</summary>
    public static PontoonGame<int> currentHighScore =>
        playersState.Map(ps => ps.Filter(p => !p.State.IsBust)
                                 .Choose(p => p.State.MaximumNonBustScore)
                                 .Max(0));

    /// <summary>모든 플레이어 상태 조회</summary>
    public static PontoonGame<Seq<(PontoonPlayer Player, PontoonPlayerState State)>> playersState =>
        state.Map(s => s.State).Map(gs => gs.AsIterable().ToSeq());

    /// <summary>아직 게임 중인 플레이어들</summary>
    public static PontoonGame<Seq<(PontoonPlayer Player, PontoonPlayerState State)>> activePlayersState =>
        playersState.Map(ps => ps.Filter(p => p.State.StillInTheGame()));

    /// <summary>게임 종료된 플레이어들</summary>
    public static PontoonGame<Seq<(PontoonPlayer Player, PontoonPlayerState State)>> nonActivePlayersState =>
        playersState.Map(ps => ps.Filter(p => !p.State.StillInTheGame()));

    /// <summary>모든 플레이어 목록</summary>
    public static PontoonGame<Seq<PontoonPlayer>> players =>
        playersState.Map(ps => ps.Map(p => p.Player));

    /// <summary>아직 게임 중인 플레이어 목록</summary>
    public static PontoonGame<Seq<PontoonPlayer>> activePlayers =>
        activePlayersState.Map(ps => ps.Map(p => p.Player).Strict());

    /// <summary>모든 플레이어 상태 초기화</summary>
    public static PontoonGame<Unit> initPlayers =>
        modifyPlayers(kv => kv.Map(_ => PontoonPlayerState.Zero));

    /// <summary>
    /// 승자 결정
    ///
    /// Bust 되지 않은 플레이어 중 최고 점수 플레이어를 찾습니다.
    /// 동점자가 여러 명일 수 있으므로 Seq로 반환합니다.
    /// </summary>
    public static PontoonGame<Seq<(PontoonPlayer Player, int Score)>> winners =>
        from ps in playersState
        select ps.Choose(p => p.State.MaximumNonBustScore.Map(score => (p.Player, Score: score)))
                 .OrderByDescending(p => p.Score)
                 .Select(Seq)
                 .Reduce((ss, sp) => (from s in ss
                                      from p in sp
                                      select s.Score == p.Score).ForAll(x => x)
                                         ? ss + sp
                                         : ss);

    /// <summary>플레이어 존재 확인</summary>
    public static PontoonGame<bool> playerExists(string name) =>
        playerExists(new PontoonPlayer(name));

    public static PontoonGame<bool> playerExists(PontoonPlayer player) =>
        state.Map(s => s.State)
             .Map(s => s.Find(player).IsSome);

    /// <summary>플레이어 추가</summary>
    public static PontoonGame<Unit> addPlayer(string name) =>
        iff(playerExists(name),
            Then: PontoonDisplay.playerExists(name),
            Else: from _1 in modifyPlayers(s => s.Add(new PontoonPlayer(name), PontoonPlayerState.Zero))
                  from _2 in PontoonDisplay.playerAdded(name)
                  select unit)
           .As();

    /// <summary>지연 평가 래퍼</summary>
    public static PontoonGame<A> lazy<A>(Func<PontoonGame<A>> f) =>
        unitM.Bind(_ => f());

    /// <summary>Option을 Game으로 리프트 (None = 게임 취소)</summary>
    public static PontoonGame<A> lift<A>(Option<A> ma) =>
        PontoonGame<A>.Lift(ma);

    /// <summary>IO를 Game으로 리프트</summary>
    public static PontoonGame<A> liftIO<A>(IO<A> ma) =>
        PontoonGame<A>.LiftIO(ma);

    /// <summary>상태의 일부를 읽기</summary>
    public static PontoonGame<A> gets<A>(Func<PontoonGameState, A> f) =>
        new(StateT.gets<OptionT<IO>, PontoonGameState, A>(f));

    /// <summary>상태 변환 적용</summary>
    public static PontoonGame<Unit> modify(Func<PontoonGameState, PontoonGameState> f) =>
        new(StateT.modify<OptionT<IO>, PontoonGameState>(f));

    /// <summary>플레이어 맵 수정</summary>
    public static PontoonGame<Unit> modifyPlayers(
        Func<HashMap<PontoonPlayer, PontoonPlayerState>, HashMap<PontoonPlayer, PontoonPlayerState>> f) =>
        modify(s => s with { State = f(s.State) });
}

#endregion

// ============================================================================
// Player 컨텍스트 관리
// ============================================================================

#region Player Context

/// <summary>
/// 플레이어 관련 연산
///
/// Reader-like 패턴으로 현재 플레이어 컨텍스트를 관리합니다.
/// with() 메서드로 특정 플레이어를 "현재 플레이어"로 설정하고
/// 해당 컨텍스트에서 연산을 실행한 후 원래 상태로 복원합니다.
/// </summary>
public static class PontoonPlayerOps
{
    /// <summary>
    /// 플레이어 컨텍스트에서 연산 실행
    ///
    /// 1. 현재 플레이어 백업
    /// 2. 지정 플레이어를 현재 플레이어로 설정
    /// 3. 연산 실행
    /// 4. 원래 플레이어 복원
    /// </summary>
    public static PontoonGame<A> with<A>(PontoonPlayer player, PontoonGame<A> ma) =>
        from cp in PontoonGame.gets(s => s.CurrentPlayer)
        from _1 in setCurrent(player)
        from rs in ma >> setCurrent(cp)
        select rs;

    /// <summary>현재 플레이어 조회 (없으면 게임 취소)</summary>
    public static PontoonGame<PontoonPlayer> current =>
        from p in PontoonGame.gets(s => s.CurrentPlayer)
        from r in PontoonGame.lift(p)
        select r;

    /// <summary>현재 플레이어 설정</summary>
    public static PontoonGame<Unit> setCurrent(PontoonPlayer player) =>
        PontoonGame.modify(s => s with { CurrentPlayer = player }).As();

    public static PontoonGame<Unit> setCurrent(Option<PontoonPlayer> player) =>
        PontoonGame.modify(s => s with { CurrentPlayer = player }).As();

    /// <summary>현재 플레이어의 상태 조회</summary>
    public static PontoonGame<PontoonPlayerState> state =>
        from pl in current
        from s1 in PontoonGame.state.Map(s => s.State)
        from s2 in PontoonGame.lift(s1.Find(pl))
        select s2;

    /// <summary>현재 플레이어의 카드와 점수 표시</summary>
    public static PontoonGame<Unit> showCards =>
        from st in state
        from score in PontoonGame.currentHighScore
        from cards in PontoonDisplay.showCardsAndScores(st.Cards, st.Scores, score)
        select unit;

    /// <summary>현재 플레이어 상태 수정</summary>
    static PontoonGame<Unit> modify(Func<PontoonPlayerState, PontoonPlayerState> f) =>
        from p in current
        from s in state
        from _ in PontoonGame.modifyPlayers(s1 => s1.SetItem(p, f(s)))
        select unit;

    /// <summary>현재 플레이어에게 카드 추가</summary>
    public static PontoonGame<Unit> addCard(PontoonCard card) =>
        modify(p => p.AddCard(card));

    /// <summary>현재 플레이어 Stick 처리</summary>
    public static PontoonGame<Unit> stick =>
        modify(p => p.Stick());

    /// <summary>현재 플레이어 Bust 여부</summary>
    public static PontoonGame<bool> isBust =>
        state.Map(p => p.IsBust);
}

/// <summary>
/// 복수 플레이어 일괄 처리
///
/// 여러 플레이어에 대해 동일한 연산을 순회하며 실행합니다.
/// </summary>
public static class PontoonPlayersOps
{
    /// <summary>플레이어 목록에 대해 연산 실행 (결과 무시)</summary>
    public static PontoonGame<Unit> with<A>(PontoonGame<Seq<PontoonPlayer>> playersM, PontoonGame<A> ma) =>
        playersM.Bind(ps => with(ps, ma))
                .IgnoreF()
                .As();

    /// <summary>플레이어 목록에 대해 연산 실행 (결과 무시)</summary>
    public static PontoonGame<Unit> with<A>(Seq<PontoonPlayer> players, PontoonGame<A> ma) =>
        players.TraverseM(p => PontoonPlayerOps.with(p, ma))
               .Map(_ => unit)
               .As();

    /// <summary>플레이어 목록에 대해 연산 실행 (결과 수집)</summary>
    public static PontoonGame<Seq<A>> map<A>(PontoonGame<Seq<PontoonPlayer>> playersM, PontoonGame<A> ma) =>
        playersM.Bind(ps => map(ps, ma));

    /// <summary>플레이어 목록에 대해 연산 실행 (결과 수집)</summary>
    public static PontoonGame<Seq<A>> map<A>(Seq<PontoonPlayer> players, PontoonGame<A> ma) =>
        players.TraverseM(p => PontoonPlayerOps.with(p, ma))
               .As();
}

#endregion

// ============================================================================
// Deck 연산
// ============================================================================

#region Deck Operations

/// <summary>
/// 덱 관련 연산
/// </summary>
public static class PontoonDeckOps
{
    /// <summary>
    /// 덱 셔플
    ///
    /// IO를 사용하여 랜덤하게 셔플된 새 덱을 생성하고
    /// 게임 상태를 업데이트합니다.
    /// </summary>
    public static PontoonGame<Unit> shuffle =>
        from deck in generate
        from _ in put(deck)
        select unit;

    /// <summary>현재 덱 조회</summary>
    public static PontoonGame<PontoonDeck> deck =>
        PontoonGame.gets(g => g.Deck);

    /// <summary>남은 카드 수</summary>
    public static PontoonGame<int> cardsRemaining =>
        deck.Map(d => d.Cards.Count);

    /// <summary>
    /// 카드 한 장 딜
    ///
    /// 덱이 비었으면 게임 취소 (None 반환)
    /// </summary>
    public static PontoonGame<PontoonCard> deal =>
        from d in deck
        from x in when(d.Cards.IsEmpty, PontoonDisplay.deckFinished)
        from c in PontoonGame.lift(d.Cards.Head)
        from _ in put(new PontoonDeck(d.Cards.Tail))
        select c;

    /// <summary>덱 상태 업데이트</summary>
    public static PontoonGame<Unit> put(PontoonDeck deck) =>
        PontoonGame.modify(g => g with { Deck = deck });

    /// <summary>
    /// 셔플된 덱 생성 (IO 연산)
    ///
    /// 52장의 카드를 생성하고 Fisher-Yates 알고리즘으로 셔플합니다.
    /// </summary>
    static IO<PontoonDeck> generate =>
        IO.lift(() =>
        {
            var random = new Random((int)DateTime.Now.Ticks);
            var array = LanguageExt.List.generate(52, ix => new PontoonCard(ix)).ToArray();
            random.Shuffle(array);
            return new PontoonDeck(array.ToSeqUnsafe());
        });
}

#endregion

// ============================================================================
// Console IO 래퍼
// ============================================================================

#region Console IO

/// <summary>
/// 콘솔 입출력을 IO 모나드로 래핑
///
/// 부수효과를 명시적으로 표현하여
/// 순수 함수와 불순 함수를 명확히 구분합니다.
/// </summary>
public static class PontoonConsole
{
    public static IO<Unit> emptyLine =>
        lift(System.Console.WriteLine);

    public static IO<Unit> writeLine(string line) =>
        lift(() => System.Console.WriteLine(line));

    public static IO<string> readLine =>
        lift(System.Console.ReadLine).Map(ln => ln ?? "");

    public static IO<ConsoleKeyInfo> readKey =>
        IO.lift(System.Console.ReadKey) >> emptyLine;
}

#endregion

// ============================================================================
// Display (UI 메시지)
// ============================================================================

#region Display

/// <summary>
/// UI 메시지 출력
///
/// 모든 출력을 Game 모나드 컨텍스트에서 수행합니다.
/// IO가 Game으로 암시적 변환됩니다.
/// </summary>
public static class PontoonDisplay
{
    public static readonly PontoonGame<Unit> introduction =
        PontoonConsole.writeLine("게임을 시작합니다...");

    public static readonly PontoonGame<Unit> askPlayerNames =
        PontoonConsole.writeLine("플레이어 이름을 입력하세요 (완료하려면 빈 입력):");

    public static readonly PontoonGame<Unit> askPlayAgain =
        PontoonConsole.writeLine("다시 하시겠습니까? (Y/N)");

    public static readonly PontoonGame<Unit> deckFinished =
        PontoonConsole.writeLine("덱에 카드가 없습니다!");

    public static readonly PontoonGame<Unit> cardsRemaining =
        PontoonDeckOps.cardsRemaining.Bind(remain =>
            PontoonConsole.writeLine($"덱에 {remain}장의 카드가 남았습니다."));

    public static readonly PontoonGame<Unit> bust =
        PontoonConsole.writeLine("\tBust! (21점 초과)");

    public static PontoonGame<Unit> playerExists(string name) =>
        PontoonConsole.writeLine($"'{name}' 플레이어가 이미 존재합니다.") >>
        PontoonConsole.writeLine("다른 이름을 입력하세요.");

    public static PontoonGame<Unit> playerAdded(string name) =>
        PontoonConsole.writeLine($"'{name}'님이 게임에 참가했습니다.");

    public static PontoonGame<Unit> playerStates(
        Seq<(PontoonPlayer Player, PontoonPlayerState State)> players) =>
        players.Traverse(p => playerState(p.Player, p.State)).Map(_ => unit).As();

    public static PontoonGame<Unit> playerState(PontoonPlayer player, PontoonPlayerState state) =>
        state.StickState
            ? PontoonConsole.writeLine($"{player.Name}: {state.Cards}, 가능한 점수: {state.Scores} [STICK]")
            : PontoonConsole.writeLine($"{player.Name}: {state.Cards}, 가능한 점수: {state.Scores}");

    public static PontoonGame<Unit> winners(Seq<(PontoonPlayer Player, int Score)> winners) =>
        winners switch
        {
            [] => everyoneIsBust,
            [var p] => PontoonConsole.writeLine($"\n*** {p.Player.Name}님이 {p.Score}점으로 승리했습니다! ***"),
            var ps => PontoonConsole.writeLine(
                $"\n*** {ps.Map(p => p.Player.Name).ToFullString()}가 {ps[0].Score}점으로 공동 승리! ***")
        };

    public static PontoonGame<Unit> everyoneIsBust =>
        PontoonConsole.writeLine("\n모두 Bust! 승자가 없습니다.");

    public static readonly PontoonGame<Unit> askStickOrTwist =
        PontoonPlayerOps.current.Bind(player =>
            PontoonConsole.writeLine($"\n{player.Name}님, Stick 또는 Twist? (S/T)"));

    public static readonly PontoonGame<Unit> stickOrTwistBerate =
        PontoonConsole.writeLine("'S' 키로 Stick, 'T' 키로 Twist!");

    public static PontoonGame<Unit> showCard(PontoonCard card) =>
        PontoonConsole.writeLine($"\t받은 카드: {card}");

    public static PontoonGame<Unit> showCardsAndScores(
        Seq<PontoonCard> cards, Seq<int> scores, int highScore) =>
        PontoonConsole.writeLine($"\t카드: {cards}, 가능한 점수: {scores}, 현재 최고점: {highScore}");
}

#endregion

// ============================================================================
// 게임 로직
// ============================================================================

#region Game Logic

/// <summary>
/// 메인 게임 로직
///
/// LINQ 쿼리 구문과 >> 연산자를 사용하여
/// 게임 흐름을 선언적으로 표현합니다.
/// </summary>
public partial class PontoonGame
{
    /// <summary>
    /// 게임 메인 진입점
    ///
    /// 1. 플레이어 이름 입력
    /// 2. 덱 셔플
    /// 3. 게임 플레이
    /// </summary>
    public static K<PontoonGame, Unit> play =>
        PontoonDisplay.askPlayerNames >>
        enterPlayerNames >>
        PontoonDisplay.introduction >>
        PontoonDeckOps.shuffle >>
        playHands;

    /// <summary>플레이어 이름 반복 입력 (빈 입력까지)</summary>
    static PontoonGame<Unit> enterPlayerNames =>
        when(enterPlayerName, lazy(() => enterPlayerNames)).As();

    /// <summary>단일 플레이어 이름 입력</summary>
    static PontoonGame<bool> enterPlayerName =>
        from name in PontoonConsole.readLine
        from _ in when(notEmpty(name), addPlayer(name))
        select notEmpty(name);

    /// <summary>여러 핸드 플레이 (재시작 가능)</summary>
    static PontoonGame<Unit> playHands =>
        from _ in initPlayers >>>
                  playHand >>>
                  PontoonDisplay.askPlayAgain
        from key in PontoonConsole.readKey
        from __ in when(key.Key == ConsoleKey.Y, playHands)
        select unit;

    /// <summary>
    /// 단일 핸드 플레이
    ///
    /// 1. 카드 배분
    /// 2. 라운드 진행
    /// 3. 승자 발표
    /// </summary>
    static PontoonGame<Unit> playHand =>
        (dealHands >>
        playRound >>
        gameOver >>
        PontoonDisplay.cardsRemaining).As();

    /// <summary>모든 플레이어에게 초기 카드 2장 배분</summary>
    static PontoonGame<Unit> dealHands =>
        PontoonPlayersOps.with(players, dealHand);

    /// <summary>한 플레이어에게 카드 2장 배분</summary>
    static PontoonGame<Unit> dealHand =>
        from cs in dealCard >> dealCard
        from player in PontoonPlayerOps.current
        from state in PontoonPlayerOps.state
        from _ in PontoonDisplay.playerState(player, state)
        select unit;

    /// <summary>카드 1장 딜 후 플레이어에게 추가</summary>
    static PontoonGame<Unit> dealCard =>
        from card in PontoonDeckOps.deal
        from _ in PontoonPlayerOps.addCard(card)
        select unit;

    /// <summary>
    /// 라운드 진행
    ///
    /// 게임이 활성 상태인 동안 각 플레이어의 턴을 진행합니다.
    /// </summary>
    static PontoonGame<Unit> playRound =>
        when(isGameActive,
             from _ in PontoonPlayersOps.with(activePlayers, stickOrTwist)
             from r in playRound
             select r)
            .As();

    /// <summary>
    /// Stick 또는 Twist 선택 처리
    ///
    /// S: Stick - 현재 점수로 확정
    /// T: Twist - 카드 1장 추가
    /// 그 외: 재입력 요청
    /// </summary>
    static PontoonGame<Unit> stickOrTwist =>
        when(isGameActive,
             from _ in PontoonDisplay.askStickOrTwist >>>
                        PontoonPlayerOps.showCards
             from key in PontoonConsole.readKey
             from __ in key.Key switch
                        {
                            ConsoleKey.S => PontoonPlayerOps.stick,
                            ConsoleKey.T => twist,
                            _ => stickOrTwistBerate
                        }
             select unit)
           .As();

    /// <summary>
    /// Twist 처리
    ///
    /// 카드 1장을 받고, Bust 시 메시지 출력
    /// </summary>
    static PontoonGame<Unit> twist =>
        from card in PontoonDeckOps.deal
        from _ in PontoonPlayerOps.addCard(card) >>>
                  PontoonDisplay.showCard(card) >>>
                  when(PontoonPlayerOps.isBust, PontoonDisplay.bust)
        select unit;

    /// <summary>잘못된 입력 시 재입력 요청</summary>
    static K<PontoonGame, Unit> stickOrTwistBerate =>
        PontoonDisplay.stickOrTwistBerate >>>
        stickOrTwist;

    /// <summary>게임 종료 처리 (승자 발표)</summary>
    static PontoonGame<Unit> gameOver =>
        from ws in winners
        from ps in playersState
        from _ in PontoonDisplay.winners(ws) >>>
                  PontoonDisplay.playerStates(ps)
        select unit;
}

#endregion
