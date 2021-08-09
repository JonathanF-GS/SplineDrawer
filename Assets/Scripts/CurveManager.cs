using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public class CurveManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private float minPollPeriod = 1f / 60;
    private float timeOfPreviousPoll = 0;

    public Curve activeCurve;

    private bool curveInProgress = false;
    private bool secondNodePlaced = false;

    private Vector2 lastNodePosition = Vector2.zero;
    private Vector2 lastPolledPosition = Vector2.zero;
    private Vector2 lastDirection = Vector2.zero;

    private const float maxNodeDistance = 5f;
    private const float minNodeDistance = 0.1f;
    private float nodeDistance = 1f;

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 inputPosition = eventData.pressEventCamera.ScreenToWorldPoint(eventData.position);
        lastNodePosition = inputPosition;

        if (curveInProgress)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                EndCurve();
            }
            else if (CurveOptions.InputMethod == CurveOptions.InputMethods.controlPoints &&
                     eventData.button == PointerEventData.InputButton.Left)
            {
                PlaceNodeAtPosition(inputPosition);
            }

        }
        else if (Input.GetMouseButtonDown(0))
        {
            activeCurve = Object.Instantiate(CurveOptions.CurveType, inputPosition, Quaternion.identity)
                .GetComponent<Curve>();
            lastNodePosition = inputPosition;
            secondNodePlaced = false;
            curveInProgress = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Vector2 inputPosition = eventData.pressEventCamera.ScreenToWorldPoint(eventData.position);

        if (CurveOptions.InputMethod == CurveOptions.InputMethods.fitCurve)
        {
            PlaceNodeAtPosition(inputPosition);
            curveInProgress = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        float timeSincePrevPoll = Time.time - timeOfPreviousPoll;
        
        if (timeSincePrevPoll > minPollPeriod)
        {
            Vector2 inputPosition = eventData.pressEventCamera.ScreenToWorldPoint(eventData.position);

            if (secondNodePlaced)
            {
                activeCurve.UpdateLastNode(inputPosition);
            }

            float distanceFromLastNode = (inputPosition - lastNodePosition).magnitude;

            if (distanceFromLastNode >= minNodeDistance)
            {
                Vector2 direction = (inputPosition - lastPolledPosition).normalized;
                float directionChange = Mathf.Pow((Vector2.Dot(lastDirection, direction) + 1) / 2, 3);
                Debug.Log(directionChange);
                nodeDistance = Mathf.Max(nodeDistance * directionChange, minNodeDistance);
                //Debug.Log(nodeDistance);
                lastDirection = direction;
                lastPolledPosition = inputPosition;
            }

            if (CurveOptions.InputMethod == CurveOptions.InputMethods.fitCurve &&
                (inputPosition - lastNodePosition).magnitude > Mathf.Max(nodeDistance, minNodeDistance))
            {
                PlaceNodeAtPosition(inputPosition);
            }

            timeOfPreviousPoll = Time.time;
        }
    }

    public void EndCurve()
    {
        curveInProgress = false;
    }

    public void PlaceNodeAtPosition(Vector2 position)
    {
        secondNodePlaced = true;
        activeCurve.AddNodeAtPosition(position);
        lastNodePosition = position;
        nodeDistance = Mathf.Min(nodeDistance + maxNodeDistance / 20, maxNodeDistance);
    }
}