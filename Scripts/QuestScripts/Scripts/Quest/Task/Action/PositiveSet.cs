using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositiveSet : TaskAction
{
    public override int Run(Task task, int currentSuccess, int successCount)
    {
        return successCount > 0 ? successCount : currentSuccess;

    }
}
