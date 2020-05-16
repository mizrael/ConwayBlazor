using System;
using System.Threading.Tasks;

namespace ConwayBlazor.Models
{
    public class World
    {
        private readonly int _sizeX;
        private readonly int _sizeY;
        private readonly bool[][,] _cells;
        private int _refreshDelay;

        public World(int sizeX, int sizeY)
        {
            _sizeX = sizeX;
            _sizeY = sizeY;

            this.Generation = 0;

            _cells = new[]
            {
                new bool[sizeX, sizeY],
                new bool[sizeX, sizeY]
            };

            Reset();
            SetRefreshFrequency(10);
        }

        public void Reset()
        {
            this.Generation = 0;

            var rand = new Random();

            for (var y = 0; y != _sizeY; ++y)
            {
                for (var x = 0; x != _sizeX; ++x)
                {
                    _cells[0][x,y] = rand.NextDouble() > .6;
                    _cells[1][x,y] = rand.NextDouble() > .6;
                }
            }
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    Step();
                    NotifyStateChanged();
                    await Task.Delay(_refreshDelay);
                }
            });
        }

        public void SetRefreshFrequency(int frequency)
        {
            _refreshDelay = (int)(1000 * (1f / frequency));
        }

        private int IsNeighborAlive(int gridIndex, int proposedOffsetX, int proposedOffsetY)
        {
            var outOfBounds = proposedOffsetX < 0 || proposedOffsetX >= _sizeX || 
                                  proposedOffsetY < 0 || proposedOffsetY >= _sizeY;
            return (!outOfBounds)
                ? _cells[gridIndex][proposedOffsetX, proposedOffsetY] ? 1 : 0
                : 0;
        }

        private void Step()
        {
            var index = this.Generation & 1;
            var world = _cells[index];
            var nextGeneration = _cells[(1+this.Generation) & 1];

            this.Population = 0;
         
            for (var y = 0; y != _sizeY; ++y)
            {
                for (var x = 0; x != _sizeX; ++x)
                {
                    var aliveNeighbor = IsNeighborAlive(index, x-1, y)
                                            + IsNeighborAlive(index, x-1, y+1)
                                            + IsNeighborAlive(index, x, y+1)
                                            + IsNeighborAlive(index, x+1, y+1)
                                            + IsNeighborAlive(index, x+1, y)
                                            + IsNeighborAlive(index, x+1, y-1)
                                            + IsNeighborAlive(index, x, y-1)
                                            + IsNeighborAlive(index, x-1, y-1);

                    var isAlive = world[x, y];

                    var survives = (!isAlive && aliveNeighbor == 3) ||
                                       (isAlive && (aliveNeighbor == 2 || aliveNeighbor == 3));


                    nextGeneration[x, y] = survives;
                    this.Population += survives ? 1 : 0;
                };
            };

            this.Generation++;
        }

        public bool[,] Cells
        {
            get
            {
                var index = this.Generation & 1;
                return _cells[index];
            }
        }

        public int Generation { get; private set; }
        public int Population { get; private set; }

        public event Action OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
