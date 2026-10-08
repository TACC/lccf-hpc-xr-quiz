using UnityEngine;

public class SuperCityAnimationController : MonoBehaviour
{
    [SerializeField] private SuperCityAnalogyController analogyController;
    [SerializeField] private SuperCityPlacementController placementController;

    public void ResetAnimatorsUnder(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        Animator[] animators = root.GetComponentsInChildren<Animator>(true);

        foreach (Animator animator in animators)
        {
            if (animator == null)
            {
                continue;
            }

            animator.Rebind();
            animator.Update(0f);
        }
    }

    public void RestartAnimatorsUnder(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        Animator[] animators = root.GetComponentsInChildren<Animator>(true);

        foreach (Animator animator in animators)
        {
            if (animator == null)
            {
                continue;
            }

            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);
            animator.Play(0, 0, 0f);
            animator.Update(0f);
        }
    }

    public void ResetAllCustomAnimations()
    {
        if (analogyController.bookOrbitGroups != null)
        {
            foreach (BookOrbitGroup group in analogyController.bookOrbitGroups)
            {
                if (group != null)
                {
                    group.ResetAllBooks();
                }
            }
        }

        if (analogyController.cityLayer != null)
        {
            BuildingDarkFilter[] filters =
                analogyController.cityLayer.GetComponentsInChildren<BuildingDarkFilter>(true);

            foreach (BuildingDarkFilter filter in filters)
            {
                if (filter != null)
                {
                    filter.ResetFilter();
                }
            }

            ElevatorDoorBrokenAnimation[] elevators =
                analogyController.cityLayer.GetComponentsInChildren<ElevatorDoorBrokenAnimation>(true);

            foreach (ElevatorDoorBrokenAnimation elevator in elevators)
            {
                if (elevator != null)
                {
                    elevator.ResetElevatorDoors();
                }
            }

            VanDriveOff[] vans =
                analogyController.cityLayer.GetComponentsInChildren<VanDriveOff>(true);

            foreach (VanDriveOff van in vans)
            {
                if (van != null)
                {
                    van.ResetVan();
                }
            }
        }

        if (placementController.placementLayer != null)
        {
            BuildingDarkFilter[] filters =
                placementController.placementLayer.GetComponentsInChildren<BuildingDarkFilter>(true);

            foreach (BuildingDarkFilter filter in filters)
            {
                if (filter != null)
                {
                    filter.ResetFilter();
                }
            }

            ElevatorDoorBrokenAnimation[] elevators =
                placementController.placementLayer.GetComponentsInChildren<ElevatorDoorBrokenAnimation>(true);

            foreach (ElevatorDoorBrokenAnimation elevator in elevators)
            {
                if (elevator != null)
                {
                    elevator.ResetElevatorDoors();
                }
            }

            VanDriveOff[] vans =
                placementController.placementLayer.GetComponentsInChildren<VanDriveOff>(true);

            foreach (VanDriveOff van in vans)
            {
                if (van != null)
                {
                    van.ResetVan();
                }
            }
        }

        if (analogyController.brokenCityPieces != null)
        {
            analogyController.brokenCityPieces.ResetCity();
        }
    }
}
