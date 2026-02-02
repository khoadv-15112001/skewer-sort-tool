namespace MyGame.Modules.CardCollection
{
    public static class UIHelper
    {
        public static (int row, int col) GetGridSize(int numCard, int maxRow, int maxCol)
        {
            switch (numCard)
            {
                case 1:
                    return (1, 1);
                case 2:
                    return (1, 2);
                case 3:
                    return (2, 2);
                case 4:
                    return (2, 2);
                case 5:
                    return (2, 3);
                case 6:
                    return (2, 3);
            }
            return (maxRow, maxCol);
        }
    }
}