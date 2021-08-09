
using System;
using System.Linq;
using UnityEngine;

public class Polynomial
{
    private float[] Coefficients { get; set; }
    
    public int Order
    {
        get { return Coefficients.Length - 1; }
    }

    public Polynomial(float[] coeffs)
    {
        Coefficients = coeffs;
    }

    public float Evaluate(float value)
    {
        float result = GetCoefficient(0);

        float powerOfValue = value;
        for (int i = 1; i <= Order; ++i)
        {
            result += GetCoefficient(i) * powerOfValue;
            powerOfValue *= value;
        }

        return result;
    }

    public float GetCoefficient(int order)
    {
        if (order < Coefficients.Length)
        {
            return Coefficients[order];
        }
        else
        {
            return 0;
        }
    }

    public Monomial GetLeadingTerm()
    {
        return new Monomial(Order, GetCoefficient(Order));
    }

    public Polynomial RemoveLeadingTerm()
    {
        float[] newCoefficients = Coefficients.Take(Order).ToArray();
        return new Polynomial(newCoefficients);
    }

    public Polynomial MultiplyByConstant(float multiplier)
    {
        if (multiplier == 0)
        {
            return new Polynomial(new float[] { 0 });
        }

        float[] newCoefficients = new float[Order + 1];
        for (int i = 0; i <= Order; i++)
        {
            newCoefficients[i] = Coefficients[i] * multiplier;
        }

        return new Polynomial(newCoefficients);
    }

    public Polynomial MultiplyByMonomial(Monomial mono)
    {
        float[] newCoefficients = new float[this.Order + mono.Order + 1];
        Array.Copy(Coefficients, 0, newCoefficients, mono.Order, Order + 1);
        Polynomial newPoly = new Polynomial(newCoefficients);
        return newPoly.MultiplyByConstant(mono.Coefficient);
    }

    public static Polynomial Sum(Polynomial poly1, Polynomial poly2)
    {
        if (poly1.Order < poly2.Order)
        {
            return Sum(poly2, poly1);
        }

        int resultOrder = poly1.Order;
        if (poly1.Order == poly2.Order)
        {
            while (resultOrder >= 0 && Mathf.Abs(poly1.GetCoefficient(resultOrder) + poly2.GetCoefficient(resultOrder)) <= float.Epsilon)
            {
                --resultOrder;
            }

            if (resultOrder < 0)
            {
                float[] zeroCoeff = { 0 };
                return new Polynomial(zeroCoeff);
            }
        }
        float[] resultCoefficients = new float[resultOrder + 1];
        for (int i = 0; i <= resultOrder; ++i)
        {
            if (i <= poly2.Order)
                resultCoefficients[i] = poly1.GetCoefficient(i) + poly2.GetCoefficient(i);
            else
                resultCoefficients[i] = poly1.GetCoefficient(i);
        }
        Polynomial result = new Polynomial(resultCoefficients);
        return result;
    }

    public static Polynomial Product(Polynomial poly1, Polynomial poly2)
    {
        if (poly2.Order == 0)
        {
            return poly1.MultiplyByConstant(poly2.GetCoefficient(0));
        }
        
        Monomial leadingTerm = poly2.GetLeadingTerm();
        Polynomial remainingTerms = poly2.RemoveLeadingTerm();

        return Sum(Product(poly1, leadingTerm), Product(poly1, remainingTerms));
    }
    
    public static Polynomial Product(Monomial mono, Polynomial poly)
    {
        return poly.MultiplyByMonomial(mono);
    }

    public static Polynomial Product(Polynomial poly, Monomial mono)
    {
        return Product(mono, poly);
    }
}