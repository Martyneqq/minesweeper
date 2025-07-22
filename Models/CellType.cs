using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minesweeper
{
    public enum CellType
    {
        EMPTY = 0,
        MINE = -1,
        BOOM = -2,
        ATOM = -5
    }
}
