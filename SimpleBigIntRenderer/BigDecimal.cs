namespace Mandelbrot.SimpleBigIntRenderer;
[Serializable]
public struct BigDecimal : IComparable, IComparable<BigDecimal>
{
    public static readonly bool AlwaysTruncate = false;
    public static readonly int Precision = 10;

    private BigInteger mantissa;

    public BigInteger Mantissa
    {
        get => mantissa;
        set => mantissa = new BigInteger(value);
    }

    public int exponent;

    public BigDecimal(BigInteger mantissa, int exponent)
    {
        this.mantissa = mantissa;
        this.exponent = exponent;

        if (AlwaysTruncate) Truncate();
    }

    public void Set(BigDecimal d)
    {
        mantissa = new BigInteger(d.mantissa);
        exponent = d.exponent;
    }
    
    public void Normalize()
    {
        if (Mantissa.IsZero())
            exponent = 0;
        else
            while (Mantissa.Mod10() > 0)
            {
                Mantissa.DivByPow10(1);
                exponent++;
            }
    }
    public void Truncate()
    {
        int numDigits = Mantissa.NumDigits;
        if (numDigits > Precision)
        {
            int powTen = numDigits - Precision;
            BigInteger b = Mantissa.DivByPow10(powTen);
            Mantissa = b;
            exponent += powTen;
        }
    }

    #region Conversions

    public static implicit operator BigDecimal(int value)
    {
        return new BigDecimal(value, 0);
    }

    public static implicit operator BigDecimal(double value)
    {
        BigInteger mantissa = value;
        int exponent = 0;
        double scaleFactor = 1;
        while (Math.Abs(value * scaleFactor - (double) mantissa) > 0)
        {
            exponent -= 1;
            scaleFactor *= 10;
            mantissa = value * scaleFactor;
        }

        return new BigDecimal(mantissa, exponent);
    }

    public static explicit operator double(BigDecimal value)
    {
        return (double) value.Mantissa * Math.Pow(10, value.exponent);
    }

    public static explicit operator float(BigDecimal value)
    {
        return Convert.ToSingle((double) value);
    }

    public static explicit operator int(BigDecimal value)
    {
        return (int) new BigInteger(value.Mantissa).MulByPow10(value.exponent);
    }

    public static explicit operator uint(BigDecimal value)
    {
        return (uint) new BigInteger(value.Mantissa).MulByPow10(value.exponent);
    }

    #endregion

    #region Operators

    public static BigDecimal operator +(BigDecimal value)
    {
        return value;
    }

    public void Zero()
    {
        Mantissa = 0;
        exponent = 0;
    }
    private static BigInteger AlignExponent(BigDecimal value, BigDecimal reference)
    {
        BigInteger b = new BigInteger(value.Mantissa).MulByPow10(value.exponent - reference.exponent);
        return b;
    }

    private static BigInteger AlignExponentInPlace(BigDecimal value, BigDecimal reference)
    {
        value.mantissa.MulByPow10(value.exponent - reference.exponent);
        return value.mantissa;
    }

    public void Add(BigDecimal value)
    {
        if (exponent > value.exponent)
        {
            mantissa.MulByPow10(exponent - value.exponent);
            mantissa.Add(value.mantissa);
            exponent = value.exponent;
        }
        else if (exponent < value.exponent)
        {
            mantissa.Add(AlignExponent(value, this));
        }
        else
        {
            mantissa.Add(value.Mantissa);
        }
    }

    public void Multiply(BigDecimal value)
    {
        mantissa.Multiply(value.Mantissa);
        exponent += value.exponent;
    }

    public static BigDecimal operator -(BigDecimal value)
    {
        BigDecimal b = 0;
        b.Set(value);
        b.mantissa.Negate();
        return b;
    }

    public static BigDecimal operator ++(BigDecimal value)
    {
        return value + 1;
    }

    public static BigDecimal operator --(BigDecimal value)
    {
        return value - 1;
    }

    public static BigDecimal operator +(BigDecimal left, BigDecimal right)
    {
        return Add(left, right);
    }

    public static BigDecimal operator -(BigDecimal left, BigDecimal right)
    {
        return Add(left, -right);
    }

    private static BigDecimal Add(BigDecimal left, BigDecimal right)
    {
        BigDecimal b = 0;
        b.Set(left);

        return left.exponent > right.exponent
            ? new BigDecimal(AlignExponentInPlace(b, right).Add(right.Mantissa), right.exponent)
            : new BigDecimal(AlignExponent(right, left).Add(left.Mantissa), left.exponent);
    }

    public static BigDecimal operator *(BigDecimal left, BigDecimal right)
    {
        return new BigDecimal(left.Mantissa * right.Mantissa, left.exponent + right.exponent);
    }

    public static BigDecimal operator /(BigDecimal dividend, BigDecimal divisor)
    {
        var exponentChange = Precision - (dividend.Mantissa.NumDigits - divisor.Mantissa.NumDigits);
        if (exponentChange < 0) exponentChange = 0;
        BigInteger b = new BigInteger(dividend.Mantissa);
        b.MulByPow10(exponentChange);
        return new BigDecimal(b.Divide(divisor.Mantissa), dividend.exponent - divisor.exponent - exponentChange);
    }

    public static bool operator ==(BigDecimal left, BigDecimal right)
    {
        return left.exponent == right.exponent && left.Mantissa == right.Mantissa;
    }

    public static bool operator !=(BigDecimal left, BigDecimal right)
    {
        return left.exponent != right.exponent || left.Mantissa != right.Mantissa;
    }

    public static bool operator <(BigDecimal left, BigDecimal right)
    {
        return left.exponent > right.exponent
            ? AlignExponent(left, right) < right.Mantissa
            : left.Mantissa < AlignExponent(right, left);
    }

    public static bool operator >(BigDecimal left, BigDecimal right)
    {
        return left.exponent > right.exponent
            ? AlignExponent(left, right) > right.Mantissa
            : left.Mantissa > AlignExponent(right, left);
    }

    public static bool operator <=(BigDecimal left, BigDecimal right)
    {
        return left.exponent > right.exponent
            ? AlignExponent(left, right) <= right.Mantissa
            : left.Mantissa <= AlignExponent(right, left);
    }

    public static bool operator >=(BigDecimal left, BigDecimal right)
    {
        return left.exponent > right.exponent
            ? AlignExponent(left, right) >= right.Mantissa
            : left.Mantissa >= AlignExponent(right, left);
    }

    #endregion

    public override string ToString()
    {
        return string.Concat(Mantissa.ToString(), "E", exponent);
    }

    public bool Equals(BigDecimal other)
    {
        return other.Mantissa.Equals(Mantissa) && other.exponent == exponent;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is BigDecimal && Equals((BigDecimal) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Mantissa.GetHashCode() * 397) ^ exponent;
        }
    }

    public int CompareTo(object? obj)
    {
        if (obj is not BigDecimal @decimal) throw new ArgumentException();
        return CompareTo(@decimal);
    }

    public int CompareTo(BigDecimal other)
    {
        return this < other ? -1 : this > other ? 1 : 0;
    }
}