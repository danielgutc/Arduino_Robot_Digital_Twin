using MeEncoderOnBoard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DifferentialDrive
{
    public interface IDifferentialDrive
    {
        public void SetElements(Rigidbody vehicleBody, IMeEncoderOnBoard leftMotor, IMeEncoderOnBoard rightMotor);
    }
}
