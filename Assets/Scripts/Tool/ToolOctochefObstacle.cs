using System.Collections.Generic;

namespace Tool
{
    public class ToolOctochefObstacle : ToolObstacleBase
    {
        public override void SetGrills(List<ToolGrill> toolGrills)
        {
            base.SetGrills(toolGrills);

            transform.position = toolGrills[0].transform.position;
            transform.SetParent(toolGrills[0].transform);
        }

    }
}