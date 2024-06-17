using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace koorsach
{
    public class GameLogic
    {
        public const int BoardRows = 7;
        public const int BoardCols = 7;
        public const int CellSize = 50;
        private PieceType[,] board;
        private bool[,] validCells;
        private List<Point> foxes;
        private List<Point> chickens;
        private bool isFoxTurn = false;
        private Point? selectedChicken;
        private Form1 form;

        public GameLogic(Form1 form)
        {
            this.form = form;
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            board = new PieceType[BoardRows, BoardCols];
            validCells = new bool[BoardRows, BoardCols];
            foxes = new List<Point>();
            chickens = new List<Point>();

            // Initialize the board with empty and invalid cells
            for (int row = 0; row < BoardRows; row++)
            {
                for (int col = 0; col < BoardCols; col++)
                {
                    board[row, col] = PieceType.Empty;
                    validCells[row, col] = false;
                }
            }

            // Set valid cells based on the new board configuration
            SetValidCells(new List<Point>
            {
                new Point(0, 2), new Point(0, 3), new Point(0, 4),
                new Point(1, 2), new Point(1, 3), new Point(1, 4),
                new Point(2, 0), new Point(2, 1), new Point(2, 2), new Point(2, 3), new Point(2, 4), new Point(2, 5), new Point(2, 6),
                new Point(3, 0), new Point(3, 1), new Point(3, 2), new Point(3, 3), new Point(3, 4), new Point(3, 5), new Point(3, 6),
                new Point(4, 0), new Point(4, 1), new Point(4, 2), new Point(4, 3), new Point(4, 4), new Point(4, 5), new Point(4, 6),
                new Point(5, 2), new Point(5, 3), new Point(5, 4),
                new Point(6, 2), new Point(6, 3), new Point(6, 4)
            });

            // Place chickens based on the new board configuration
            PlaceChickens(new List<Point>
            {
                new Point(3, 0), new Point(3, 1), new Point(3, 2), new Point(3, 3), new Point(3, 4), new Point(3, 5), new Point(3, 6),
                new Point(4, 0), new Point(4, 1), new Point(4, 2), new Point(4, 3), new Point(4, 4), new Point(4, 5), new Point(4, 6),
                new Point(5, 2), new Point(5, 3), new Point(5, 4),
                new Point(6, 2), new Point(6, 3), new Point(6, 4)
            });

            // Place foxes based on the new board configuration
            PlaceFoxes(new List<Point>
            {
                new Point(2, 2), new Point(2, 4)
            });
        }

        private void SetValidCells(List<Point> validPositions)
        {
            foreach (var pos in validPositions)
            {
                validCells[pos.X, pos.Y] = true;
            }
        }

        private void PlaceChickens(List<Point> chickenPositions)
        {
            foreach (var pos in chickenPositions)
            {
                board[pos.X, pos.Y] = PieceType.Chicken;
                chickens.Add(pos);
            }
        }

        private void PlaceFoxes(List<Point> foxPositions)
        {
            foreach (var pos in foxPositions)
            {
                board[pos.X, pos.Y] = PieceType.Fox;
                foxes.Add(pos);
            }
        }

        public void DrawBoard(Graphics g)
        {
            for (int row = 0; row < BoardRows; row++)
            {
                for (int col = 0; col < BoardCols; col++)
                {
                    Rectangle cell = new Rectangle(col * CellSize, row * CellSize, CellSize, CellSize);

                    if (validCells[row, col])
                    {
                        g.FillRectangle(Brushes.White, cell);
                        g.DrawRectangle(Pens.Black, cell);

                        switch (board[row, col])
                        {
                            case PieceType.Chicken:
                                g.FillEllipse(Brushes.Yellow, cell);
                                break;
                            case PieceType.Fox:
                                g.FillEllipse(Brushes.Red, cell);
                                break;
                        }
                    }
                    else
                    {
                        g.FillRectangle(Brushes.Gray, cell);
                    }
                }
            }
        }

        public void HandleMouseClick(int x, int y)
        {
            int col = x / CellSize;
            int row = y / CellSize;

            if (validCells[row, col])
            {
                if (!isFoxTurn)
                {
                    // Handle chicken movement
                    if (board[row, col] == PieceType.Chicken)
                    {
                        // Select chicken to move
                        selectedChicken = new Point(row, col);
                    }
                    else if (selectedChicken.HasValue && IsValidMove(selectedChicken.Value, new Point(row, col)))
                    {
                        // Move selected chicken if the move is valid
                        MovePiece(selectedChicken.Value, new Point(row, col));
                        selectedChicken = null;
                        CheckChickenWin();
                        isFoxTurn = true;
                        FoxMove();
                    }
                }
            }
        }

        private bool IsValidMove(Point from, Point to)
        {
            if (!IsValidPosition(to.X, to.Y) || board[to.X, to.Y] != PieceType.Empty)
                return false;

            int dx = to.X - from.X;
            int dy = to.Y - from.Y;

            if (board[from.X, from.Y] == PieceType.Chicken)
            {
                // Chicken can move up, left or right, but not backwards
                return (dx == -1 && dy == 0) || (dx == 0 && Math.Abs(dy) == 1);
            }
            else if (board[from.X, from.Y] == PieceType.Fox)
            {
                // Fox can move in all directions
                return Math.Abs(dx) + Math.Abs(dy) == 1;
            }

            return false;
        }

        private void MovePiece(Point from, Point to)
        {
            board[to.X, to.Y] = board[from.X, from.Y];
            board[from.X, from.Y] = PieceType.Empty;

            if (board[to.X, to.Y] == PieceType.Chicken)
            {
                chickens.Remove(from);
                chickens.Add(to);
            }
            else if (board[to.X, to.Y] == PieceType.Fox)
            {
                foxes.Remove(from);
                foxes.Add(to);
            }

            form.Invalidate();
        }

        private List<Point> GetValidMoves(Point piece)
        {
            List<Point> validMoves = new List<Point>();

            int[,] directions = {
                { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 }
            };

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int newX = piece.X + directions[i, 0];
                int newY = piece.Y + directions[i, 1];

                Point newPoint = new Point(newX, newY);

                if (IsValidMove(piece, newPoint))
                {
                    validMoves.Add(newPoint);
                }
            }

            return validMoves;
        }

        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < BoardRows && y >= 0 && y < BoardCols && validCells[x, y];
        }

        private void FoxMove()
        {
            bool foxMoved = false;
            foreach (var fox in foxes)
            {
                if (CanFoxEat(fox))
                {
                    // Perform the eating logic
                    List<Point> eatingMoves = GetFoxEatingMoves(fox);
                    Point bestMove = GetBestEatingMove(eatingMoves);
                    MoveFoxAndEatChicken(fox, bestMove);
                    foxMoved = true;
                    break;
                }
            }

            if (!foxMoved)
            {
                // If no fox can eat, move a fox to a valid position
                foreach (var fox in foxes)
                {
                    var validMoves = GetValidMoves(fox);
                    if (validMoves.Count > 0)
                    {
                        // Choose a valid move and perform it
                        MovePiece(fox, validMoves[0]);
                        foxMoved = true;
                        break;
                    }
                }
            }

            // Check for foxes' win condition
            CheckFoxWin();

            isFoxTurn = false; // Switch back to chicken's turn
        }

        private bool CanFoxEat(Point fox)
        {
            int[,] directions = {
                { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 }
            };

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int newX = fox.X + directions[i, 0];
                int newY = fox.Y + directions[i, 1];

                if (IsValidPosition(newX, newY) && board[newX, newY] == PieceType.Chicken)
                {
                    int jumpX = newX + directions[i, 0];
                    int jumpY = newY + directions[i, 1];

                    if (IsValidPosition(jumpX, jumpY) && board[jumpX, jumpY] == PieceType.Empty)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private List<Point> GetFoxEatingMoves(Point fox)
        {
            List<Point> eatingMoves = new List<Point>();

            int[,] directions = {
                { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 }
            };

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int newX = fox.X + directions[i, 0];
                int newY = fox.Y + directions[i, 1];

                if (IsValidPosition(newX, newY) && board[newX, newY] == PieceType.Chicken)
                {
                    int jumpX = newX + directions[i, 0];
                    int jumpY = newY + directions[i, 1];

                    if (IsValidPosition(jumpX, jumpY) && board[jumpX, jumpY] == PieceType.Empty)
                    {
                        eatingMoves.Add(new Point(newX, newY));
                    }
                }
            }

            return eatingMoves;
        }

        private Point GetBestEatingMove(List<Point> eatingMoves)
        {
            // For now, just return the first eating move
            return eatingMoves[0];
        }

        private void MoveFoxAndEatChicken(Point fox, Point chicken)
        {
            int jumpX = 2 * chicken.X - fox.X;
            int jumpY = 2 * chicken.Y - fox.Y;

            board[jumpX, jumpY] = PieceType.Fox;
            board[fox.X, fox.Y] = PieceType.Empty;
            board[chicken.X, chicken.Y] = PieceType.Empty;

            foxes.Remove(fox);
            foxes.Add(new Point(jumpX, jumpY));

            chickens.Remove(chicken);

            // Check for foxes' win condition
            CheckFoxWin();
        }

        private void CheckChickenWin()
        {
            int count = 0;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 2; col <= 4; col++)
                {
                    if (board[row, col] == PieceType.Chicken)
                    {
                        count++;
                    }
                }
            }

            if (count >= 9)
            {
                MessageBox.Show("Chickens win!");
                Application.Exit();
            }
        }

        private void CheckFoxWin()
        {
            if (chickens.Count <= 8)
            {
                MessageBox.Show("Foxes win!");
                Application.Exit();
            }
        }
    }
}