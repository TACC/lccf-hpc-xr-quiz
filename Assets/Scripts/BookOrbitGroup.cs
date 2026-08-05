using System.Collections;
using UnityEngine;

public class BookOrbitGroup : MonoBehaviour
{
    private BookPlanetOrbit[] bookOrbits;

    void Awake()
    {
        RefreshBookOrbits();
    }

    public void RefreshBookOrbits()
    {
        bookOrbits = GetComponentsInChildren<BookPlanetOrbit>(true);
    }

    public void SaveCurrentBookPositionsAsHome()
    {
        RefreshBookOrbits();

        foreach (BookPlanetOrbit orbit in bookOrbits)
        {
            if (orbit != null)
            {
                orbit.SaveCurrentPositionAsOriginal();
            }
        }
    }

    public void StartAllBookOrbits()
    {
        RefreshBookOrbits();

        foreach (BookPlanetOrbit orbit in bookOrbits)
        {
            if (orbit != null)
            {
                orbit.StartOrbit();
            }
        }
    }

    public void StopAllBookOrbits()
    {
        RefreshBookOrbits();

        foreach (BookPlanetOrbit orbit in bookOrbits)
        {
            if (orbit != null)
            {
                orbit.StopOrbit();
            }
        }
    }

    public IEnumerator SlideAllBooksBack(float duration)
    {
        RefreshBookOrbits();

        foreach (BookPlanetOrbit orbit in bookOrbits)
        {
            if (orbit != null)
            {
                orbit.StartCoroutine(orbit.SlideBackToOriginalPosition(duration));
            }
        }

        yield return new WaitForSeconds(duration);
    }

    public void ResetAllBooks()
    {
        StopAllCoroutines();
        RefreshBookOrbits();

        foreach (BookPlanetOrbit orbit in bookOrbits)
        {
            if (orbit != null)
            {
                orbit.ResetOrbit();
            }
        }
    }

}