using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleInfoPlugin.Models
{
    public enum AttackType
    {
        通常, //1.0
        連撃, //1.2*2
        カットイン主主,    //1.5
        カットイン主徹,    //1.3
        カットイン主電,    //1.2
        カットイン主副,    //1.1
        カットイン雷,     //1.5*2
        カットイン主主主,   //2.0
        カットイン主主副,   //1.75
        カットイン主雷,    //1.3*2
    }
}
