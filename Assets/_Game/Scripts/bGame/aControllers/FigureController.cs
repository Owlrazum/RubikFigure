using System.Collections;
using UnityEngine;

public class FigureController : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(FigureStartSequence());
    }

    private IEnumerator FigureStartSequence()
    {
        FigureDelegatesContainer.FinishMeshGeneration();
        yield return null;
        FigureDelegatesContainer.FinishShuffleTransitionsGeneration();
        Figure figure = FigureDelegatesContainer.GetFigure();
        figure.StatesController.StartUpdating();
    }
}