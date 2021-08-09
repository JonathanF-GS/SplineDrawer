
using System;

public class Monomial
{
    private int _order;
    public int Order
    {
        get { return _order; }
        private set
        {
            if (value < 0)
            {
                _order = 0;
            }
            else
            {
                _order = value;
            }
        }
    }

    public float Coefficient { get; private set; }

    public Monomial(int order, float coeff)
    {
        Order = order;
        Coefficient = coeff;
    }
}