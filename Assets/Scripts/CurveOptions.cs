using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CurveOptions : MonoBehaviour
{
    public static GameObject CurveType;
    public static InputMethods InputMethod = InputMethods.fitCurve;

    public enum CurveTypes
    {
        openUniformBSpline,
        piecewiseLinear
    }
    
    public enum InputMethods
    {
        fitCurve,
        controlPoints
    }

    [SerializeField] private GameObject piecewiseLinearCurvePrefab;
    [SerializeField] private GameObject openUniformBSplinePrefab;

    void Start()
    {
        CurveType = openUniformBSplinePrefab;
        Dropdown curveTypeDropdown = GameObject.Find("Curve Type Selector").GetComponent<Dropdown>();
        Dropdown inputMethodDropdown = GameObject.Find("Input Style Selector").GetComponent<Dropdown>();
        curveTypeDropdown.onValueChanged.AddListener(delegate { UpdateCurveType((CurveTypes)curveTypeDropdown.value); });
        inputMethodDropdown.onValueChanged.AddListener(delegate { UpdateInputMethod(inputMethodDropdown.value); });
    }

    void Update()
    {
        
    }

    public void UpdateCurveType(CurveTypes typeID)
    {
        switch (typeID)
        {
            case CurveTypes.openUniformBSpline:
                CurveType = openUniformBSplinePrefab;
                break;
            case CurveTypes.piecewiseLinear:
                CurveType = piecewiseLinearCurvePrefab;
                break;
        }
    }

    public void UpdateInputMethod(int methodID)
    {
        InputMethod = (InputMethods) methodID;
    }
}