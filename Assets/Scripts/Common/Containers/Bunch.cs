using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct Bunch<T>
{
    public T Unit;
    public int Amount;

    public Bunch(T type) : this(type, 0)
    {
    }

    public Bunch(KeyValuePair<T, int> pair) : this(pair.Key, pair.Value)
    {
    }

    public Bunch(Bunch<T> field) : this(field.Unit, field.Amount)
    {
    }

    public Bunch(T unit, int amount)
    {
        Unit = unit;
        Amount = amount;
    }

    public static Bunch<T> operator *(Bunch<T> bunch, float multiply)
    {
        return new Bunch<T>(bunch.Unit, (int)(bunch.Amount * multiply));
    }

    public static Bunch<T> operator /(Bunch<T> bunch, float divide)
    {
        return new Bunch<T>(bunch.Unit, (int)(bunch.Amount * divide));
    }

    public static Bunch<T> operator +(Bunch<T> bunch, float plus)
    {
        return new Bunch<T>(bunch.Unit, (int)(bunch.Amount + plus));
    }

    public static Bunch<T> operator -(Bunch<T> bunch, float minus)
    {
        return new Bunch<T>(bunch.Unit, (int)(bunch.Amount - minus));
    }

    public static Bunch<T> operator *(Bunch<T> bunch, int multiply)
    {
        return new Bunch<T>(bunch.Unit, bunch.Amount * multiply);
    }

    public static Bunch<T> operator /(Bunch<T> bunch, int divide)
    {
        return new Bunch<T>(bunch.Unit, bunch.Amount * divide);
    }

    public static Bunch<T> operator +(Bunch<T> bunch, int plus)
    {
        return new Bunch<T>(bunch.Unit, bunch.Amount + plus);
    }

    public static Bunch<T> operator -(Bunch<T> bunch, int minus)
    {
        return new Bunch<T>(bunch.Unit, bunch.Amount - minus);
    }

    public static Bunch<T> operator *(Bunch<T> bunch1, Bunch<T> bunch2)
    {
        return new Bunch<T>(bunch1.Unit, bunch1.Amount * bunch2.Amount);
    }

    public static Bunch<T> operator /(Bunch<T> bunch1, Bunch<T> bunch2)
    {
        return new Bunch<T>(bunch1.Unit, bunch1.Amount / bunch2.Amount);
    }

    public static Bunch<T> operator +(Bunch<T> bunch1, Bunch<T> bunch2)
    {
        return new Bunch<T>(bunch1.Unit, bunch1.Amount + bunch2.Amount);
    }

    public static Bunch<T> operator -(Bunch<T> bunch1, Bunch<T> bunch2)
    {
        return new Bunch<T>(bunch1.Unit, bunch1.Amount - bunch2.Amount);
    }

    public KeyValuePair<T, int> ToKVP()
    {
        return new KeyValuePair<T, int>(Unit, Amount);
    }

    public override string ToString()
    {
        return "[" + Unit.ToString() + ", " + Amount.ToString() + "]";
    }
}