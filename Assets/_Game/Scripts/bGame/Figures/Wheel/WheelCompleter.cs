using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;

public class WheelCompleter : FigureCompleter
{
    private Wheel _currentWheel;
    protected override List<FigureSegmentMove> Complete(Figure figure)
    {
        var moves = base.Complete(figure);
        _currentWheel = figure as Wheel;
        for (int i = 0; i < moves.Count; i++)
        {
            // moves[i] = ((WheelTeleportMove)moves[i]);
        }
        // _currentWheel.MakeMoves(moves, null);
        StartCoroutine(CompletionSequence(1.0f / _teleportLerpSpeed));
        return null;
    }

    private IEnumerator CompletionSequence(float beforeRotatePauseTime)
    {
        yield return new WaitForSeconds(beforeRotatePauseTime);
        FigureDelegatesContainer.FigureCompleted?.Invoke();
        Vector3 rotationEuler = Vector3.zero;
        while (true)
        {
            _currentWheel.transform.Rotate(rotationEuler, Space.World);
            rotationEuler.x = s_amplitude * 2 * Time.deltaTime;
            rotationEuler.y = s_amplitude * Time.deltaTime;
            rotationEuler.z = s_amplitude * Time.deltaTime;
            yield return null;
        }
    }
} 