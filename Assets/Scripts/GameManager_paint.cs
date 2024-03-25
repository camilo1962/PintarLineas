namespace LinePaint
{
    public static class GameManager_paint
    {
        public static GameStatus gameStatus = GameStatus.Playing;
        public static int currentLevel = 0;
        public static int totalDiamonds;
    }

    public enum GameStatus
    {
        Playing,
        Complete
    }
}