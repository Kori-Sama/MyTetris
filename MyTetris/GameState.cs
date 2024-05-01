using System.IO;

namespace Tetris {
    public class GameState {
        private Block currentBlock;

        public Block CurrentBlock {
            get => currentBlock;
            private set {
                currentBlock = value;
                currentBlock.Reset();

                for (int i = 0; i < 2; i++) {
                    currentBlock.Move(1, 0);

                    if (!BlockFits()) {
                        currentBlock.Move(-1, 0);
                    }
                }
            }
        }

        public GameGrid GameGrid { get; }
        public BlockQueue BlockQueue { get; }
        public bool GameOver { get; private set; }
        public int Score { get; private set; }
        public int bestScore { get; set; }
        public GameState() {
            GameGrid = new GameGrid(22, 10);
            BlockQueue = new BlockQueue();
            CurrentBlock = BlockQueue.GetAndUpdate();
        }

        private bool BlockFits() {
            foreach (Position p in CurrentBlock.TilePositions()) {
                if (!GameGrid.IsEmpty(p.Row, p.Column)) {
                    return false;
                }
            }

            return true;
        }

        public void RotateBlockCW() {
            CurrentBlock.RotateCW();

            if (!BlockFits()) {
                CurrentBlock.RotateCCW();
            }
        }

        public void RotateBlockCCW() {
            CurrentBlock.RotateCCW();

            if (!BlockFits()) {
                CurrentBlock.RotateCW();
            }
        }

        public void MoveBlockLeft() {
            CurrentBlock.Move(0, -1);

            if (!BlockFits()) {
                CurrentBlock.Move(0, 1);
            }
        }

        public void MoveBlockRight() {
            CurrentBlock.Move(0, 1);

            if (!BlockFits()) {
                CurrentBlock.Move(0, -1);
            }
        }

        private bool IsGameOver() {
            return !(GameGrid.IsRowEmpty(0) && GameGrid.IsRowEmpty(1));
        }

        private void PlaceBlock() {
            foreach (Position p in CurrentBlock.TilePositions()) {
                GameGrid[p.Row, p.Column] = CurrentBlock.Id;
            }

            Score += GameGrid.ClearFullRows();

            if (IsGameOver()) {
                GameOver = true;
            } else {
                CurrentBlock = BlockQueue.GetAndUpdate();
            }
        }

        public void MoveBlockDown() {
            CurrentBlock.Move(1, 0);

            if (!BlockFits()) {
                CurrentBlock.Move(-1, 0);
                PlaceBlock();
            }
        }

        private int TileDropDistance(Position p) {
            int distance = 0;

            while (GameGrid.IsEmpty(p.Row + distance + 1, p.Column)) {
                distance++;
            }
            return distance;
        }

        public int BlockDropDistance() {
            int drop = GameGrid.Rows;

            foreach (Position p in CurrentBlock.TilePositions()) {
                drop = System.Math.Min(drop, TileDropDistance(p));
            }

            return drop;
        }

        public void DropBlock() {
            CurrentBlock.Move(BlockDropDistance(), 0);
            PlaceBlock();
        }

        public void WriteScore(int score) {
            try {
                FileStream fs = new FileStream(@"historybest.txt", FileMode.Open, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fs);
                writer.Write(score);
                writer.Close();
                fs.Close();
            } catch (FileNotFoundException) {
                var fs = new FileStream(@"historybest.txt", FileMode.Create, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fs);
                writer.Write(score);
                writer.Close();
                fs.Close();
            }
        }

        public void ReadScore() {
            try {
                FileStream fs = new FileStream(@"historybest.txt", FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                bestScore = System.Convert.ToInt32(sr.ReadLine());
                sr.Close();
                fs.Close();
            } catch (FileNotFoundException) {
                var fs = new FileStream(@"historybest.txt", FileMode.Create, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fs);
                writer.Write(0);
                writer.Close();
                fs.Close();
                ReadScore();
            }
        }

    }
}
