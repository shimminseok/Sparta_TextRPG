namespace Camp_FourthWeek_Basic_C__;

public class MiniGame
{
    private const int size = 10;

    private readonly List<Block> snake = new();
    private Direction direction = Direction.Right;
    private Block food = new();
    private bool isGameOver;

    private Block[,] map = new Block[size, size];

    public void StartGame()
    {
        map = CreateMap(size);
        InitSnake();
        PlaceFood();
        var inpuThread = new Thread(ReadInput);
        inpuThread.Start();
        var moveThread = new Thread(Move);
        moveThread.Start();
    }

    private void InitSnake()
    {
        snake.Add(new Block(5, 5));
        snake.Add(new Block(4, 5));
        snake.Add(new Block(3, 5));
    }

    private void DrawMap()
    {
        Console.Clear();
        for (var y = 0; y < map.GetLength(0); y++)
        {
            Console.WriteLine("==============================================================");
            for (var x = 0; x < map.GetLength(1); x++)
            {
                var isSnake = false;
                foreach (var b in snake)
                    if (b.x == x && b.y == y)
                    {
                        Console.Write("|| ■ ");
                        isSnake = true;
                        break;
                    }

                if (!isSnake)
                {
                    if (food.x == x && food.y == y)
                        Console.Write("|| ◆ ");
                    else
                        Console.Write("||    ");
                }
            }

            Console.WriteLine("||");
        }

        Console.WriteLine("==============================================================");
    }

    private Block[,] CreateMap(int _size)
    {
        var map = new Block[_size, _size];
        for (var i = 0; i < _size; i++)
        for (var j = 0; j < _size; j++)
        {
            var block = new Block(i, j);
            map[i, j] = block;
        }

        return map;
    }

    private void Move()
    {
        while (!isGameOver)
        {
            MoveSnake();
            Thread.Sleep(100);
        }
    }

    private void MoveSnake()
    {
        var head = snake.First();

        var newHeadX = head.x;
        var newHeadY = head.y;
        switch (direction)
        {
            case Direction.Up:
                newHeadY--;
                break;
            case Direction.Down:
                newHeadY++;
                break;
            case Direction.Left:
                newHeadX--;
                break;
            case Direction.Right:
                newHeadX++;
                break;
        }

        var newHead = map[newHeadX, newHeadY];


        for (var i = snake.Count - 1; i > 0; i--)
        {
            snake[i].x = snake[i - 1].x;
            snake[i].y = snake[i - 1].y;
        }

        head.x = newHeadX;
        head.y = newHeadY;
        if (head.x == food.x && head.y == food.y)
        {
            //꼬리 늘어남
            var tail = snake.Last();
            snake.Add(new Block(tail.x, tail.y));
            PlaceFood();
        }

        DrawMap();

        //자신 몸통이여도 죽고
        foreach (var b in snake)
            if (b == newHead)
            {
                isGameOver = true;
                return;
            }

        //벽이여도 죽음
        if (head.x <= 0 || head.x >= size || head.y <= 0 || head.y >= size)
        {
            Console.WriteLine("게임 오바!!");
            isGameOver = true;
        }
    }

    private void PlaceFood()
    {
        var rand = new Random();
        do
        {
            var x = rand.Next(1, size);
            var y = rand.Next(1, size);
            food = new Block(x, y);
        } while (snake.Exists(block => block.x == food.x && block.y == food.y));
    }

    public void ReadInput()
    {
        while (!isGameOver)
        {
            var keyInfo = Console.ReadKey(true);
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    if (direction != Direction.Down) direction = Direction.Up;
                    break;
                case ConsoleKey.DownArrow:
                    if (direction != Direction.Up) direction = Direction.Down;
                    break;
                case ConsoleKey.LeftArrow:
                    if (direction != Direction.Right) direction = Direction.Left;
                    break;
                case ConsoleKey.RightArrow:
                    if (direction != Direction.Left) direction = Direction.Right;
                    break;
            }
        }
    }

    private class Block
    {
        private bool isEmpty;
        public bool isFood = false;
        public int x, y;

        public Block(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public Block()
        {
        }
    }

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}