using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperCitySceneStateController : MonoBehaviour
{
    [SerializeField] private SuperCityAnalogyController analogyController;
    [SerializeField] private SuperCityPlacementController placementController;
    [SerializeField] private SuperCityAnimationController animationController;

    private class TransformHomeState
    {
        public Transform transform;
        public Transform parent;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        public bool activeSelf;
    }

    private class AnswerHomeState
    {
        public ZDraggableItem item;
        public Transform parent;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    private readonly List<TransformHomeState> sceneTransformHomeStates = 
        new List<TransformHomeState>();

    private readonly List<AnswerHomeState> answerHomeStates =
        new List<AnswerHomeState>();

    public void InitializeHomeStates()
    {
        StoreSceneTransformHomeStates();
        StoreAnswerHomeStates();
    }

    private void StoreSceneTransformHomeStates()
    {
        sceneTransformHomeStates.Clear();

        HashSet<Transform> storedTransforms = new HashSet<Transform>();

        StoreRootTransformStates(analogyController.cityAnalogies, storedTransforms);
        StoreRootTransformStates(placementController.placementGroups, storedTransforms);

        if (analogyController.brokenPlaneObject != null)
        {
            StoreTransformTree(analogyController.brokenPlaneObject.transform, storedTransforms);
        }

        if (analogyController.repairedCityModel != null)
        {
            StoreTransformTree(analogyController.repairedCityModel.transform, storedTransforms);
        }
    }

    private void StoreRootTransformStates(GameObject[] roots, HashSet<Transform> storedTransforms)
    {
        if (roots == null)
        {
            return;
        }

        foreach (GameObject root in roots)
        {
            if (root != null)
            {
                StoreTransformTree(root.transform, storedTransforms);
            }
        }
    }

    private void StoreTransformTree(Transform root, HashSet<Transform> storedTransforms)
    {
        if (root == null || storedTransforms.Contains(root))
        {
            return;
        }

        storedTransforms.Add(root);

        sceneTransformHomeStates.Add(new TransformHomeState
        {
            transform = root,
            parent = root.parent,
            localPosition = root.localPosition,
            localRotation = root.localRotation,
            localScale = root.localScale,
            activeSelf = root.gameObject.activeSelf
        });

        foreach (Transform child in root)
        {
            StoreTransformTree(child, storedTransforms);
        }
    }

    public void RestoreSceneTransformsAndAnimators()
    {
        foreach (TransformHomeState state in sceneTransformHomeStates)
        {
            if (state == null || state.transform == null)
            {
                continue;
            }

            if (state.parent != null && state.transform.parent != state.parent)
            {
                state.transform.SetParent(state.parent, false);
            }

            state.transform.localPosition = state.localPosition;
            state.transform.localRotation = state.localRotation;
            state.transform.localScale = state.localScale;
            state.transform.gameObject.SetActive(state.activeSelf);
        }

        animationController.ResetAnimatorsUnder(analogyController.cityLayer);
        animationController.ResetAnimatorsUnder(placementController.placementLayer);
    }

    private void StoreAnswerHomeStates()
    {
        answerHomeStates.Clear();

        if (analogyController.cityAnalogies == null)
        {
            return;
        }

        foreach (GameObject analogy in analogyController.cityAnalogies)
        {
            if (analogy == null)
            {
                continue;
            }

            ZDraggableItem[] choices =
                analogy.GetComponentsInChildren<ZDraggableItem>(true);

            foreach (ZDraggableItem choice in choices)
            {
                if (choice == null)
                {
                    continue;
                }

                AnswerHomeState state = new AnswerHomeState
                {
                    item = choice,
                    parent = choice.transform.parent,
                    localPosition = choice.transform.localPosition,
                    localRotation = choice.transform.localRotation,
                    localScale = choice.transform.localScale
                };

                answerHomeStates.Add(state);
            }
        }
    }

    public void RestoreAllAnswerChoices()
    {
        foreach (AnswerHomeState state in answerHomeStates)
        {
            if (state == null || state.item == null)
            {
                continue;
            }

            Transform itemTransform = state.item.transform;

            if (state.parent != null &&
                itemTransform.parent != state.parent)
            {
                itemTransform.SetParent(state.parent, false);
            }

            itemTransform.localPosition = state.localPosition;
            itemTransform.localRotation = state.localRotation;
            itemTransform.localScale = state.localScale;

            state.item.enabled = true;
            state.item.gameObject.SetActive(true);

            Collider[] colliders =
                state.item.GetComponentsInChildren<Collider>(true);

            foreach (Collider col in colliders)
            {
                if (col != null)
                {
                    col.enabled = true;
                }
            }

            Rigidbody[] rigidbodies =
                state.item.GetComponentsInChildren<Rigidbody>(true);

            foreach (Rigidbody rb in rigidbodies)
            {
                if (rb == null)
                {
                    continue;
                }

                rb.isKinematic = true;
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    public void RestoreCurrentPlacementTransforms(GameObject placementRootObject)
    {
        if (placementRootObject == null)
        {
            return;
        }

        Transform placementRoot = placementRootObject.transform;

        foreach (TransformHomeState state in sceneTransformHomeStates)
        {
            if (state == null || state.transform == null)
            {
                continue;
            }
            if (state.transform != placementRoot && !state.transform.IsChildOf(placementRoot))
            {
                continue;
            }
            if (state.parent != null && state.transform.parent != state.parent)
            {
                state.transform.SetParent(state.parent, false);
            }

            state.transform.localPosition = state.localPosition;
            state.transform.localRotation = state.localRotation;
            state.transform.localScale = state.localScale;
            state.transform.gameObject.SetActive(state.activeSelf);
        }
    }
}
