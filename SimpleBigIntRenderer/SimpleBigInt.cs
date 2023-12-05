namespace Mandelbrot.SimpleBigIntRenderer;

public class BigInteger : IComparable, IComparable<BigInteger>
{
    private const int SAMPLE_MAX_DIGITS = 128;
    private const sbyte NUM_BASE = 10;

    private readonly int maxDigits;

    private readonly sbyte[] mulTemp1;
    private readonly sbyte[] mulTemp2;

    public BigInteger(BigInteger other)
    {
        maxDigits = other.maxDigits;
        NumDigits = other.NumDigits;
        Digits = new sbyte[other.Digits.Length];
        mulTemp1 = new sbyte[Digits.Length];
        mulTemp2 = new sbyte[Digits.Length];
        Array.Copy(other.Digits, Digits, Digits.Length);
        Sign = other.Sign;
    }

    public BigInteger(long init, int maxDigits)
    {
        if (init >= 0)
        {
            Sign = 1;
        }
        else
        {
            Sign = -1;
            init = -init;
        }

        this.maxDigits = maxDigits;
        Digits = new sbyte[maxDigits * 4];
        mulTemp1 = new sbyte[Digits.Length];
        mulTemp2 = new sbyte[Digits.Length];
        if (init == 0)
        {
            NumDigits = 1;
        }
        else
        {
            NumDigits = 0;
            while (init > 0)
            {
                Digits[NumDigits++] = (sbyte) (init % NUM_BASE);
                init /= 10;
            }
        }
    }

    public BigInteger(int init) : this(init, SAMPLE_MAX_DIGITS)
    {
    }

    public BigInteger(long init) : this(init, SAMPLE_MAX_DIGITS)
    {
    }

    public BigInteger(float init) : this((long) init, SAMPLE_MAX_DIGITS)
    {
    }

    public BigInteger(double init) : this((long) init, SAMPLE_MAX_DIGITS)
    {
    }

    public sbyte[] Digits { get; }

    public int NumDigits { get; private set; }

    public int Sign { get; private set; }

    public int CompareTo(object obj)
    {
        if (ReferenceEquals(obj, null) || !(obj is BigInteger)) throw new ArgumentException();
        return CompareTo((BigInteger) obj);
    }

    public int CompareTo(BigInteger other)
    {
        return this < other ? -1 : this > other ? 1 : 0;
    }

    public static implicit operator BigInteger(int value)
    {
        return new BigInteger(value);
    }

    public static implicit operator BigInteger(double value)
    {
        return new BigInteger(value);
    }

    public static explicit operator double(BigInteger value)
    {
        double d = 0;
        for (int i = value.NumDigits; i >= 0; i--)
        {
            d *= 10.0;
            d += value.Digits[i];
        }

        return value.Sign == -1 ? -d : d;
    }


    public BigInteger Negate()
    {
        if (!IsZero()) Sign = -Sign;
        return this;
    }

    public BigInteger Zero()
    {
        Digits[0] = 0;
        NumDigits = 1;
        Sign = 1;
        return this;
    }

    public bool IsZero()
    {
        return NumDigits == 1 && Digits[0] == 0;
    }

    public bool IsIntEqual(int num)
    {
        return NumDigits == 1 && Digits[0] == num;
    }


    public BigInteger DivByPow10(int power)
    {
        if (IsZero()) return this;
        if (power >= NumDigits)
        {
            Zero();
            return this;
        }

        for (int i = power; i < NumDigits; i++) Digits[i - power] = Digits[i];
        NumDigits -= power;
        return this;
    }

    public BigInteger MulByPow10(int power)
    {
        if (IsZero()) return this;
        NumDigits += power;
        for (int i = NumDigits - 1; i >= power; i--) Digits[i] = Digits[i - power];
        for (int i = 0; i < power; i++) Digits[i] = 0;
        return this;
    }

    public int Mod10()
    {
        return Digits[0];
    }

    public static BigInteger Pow10(int power)
    {
        BigInteger b = 0;
        b.Digits[power] = 1;
        b.NumDigits = power + 1;
        return b;
    }


    public static BigInteger operator +(BigInteger value)
    {
        return value;
    }

    public static BigInteger operator -(BigInteger value)
    {
        return new BigInteger(value).Negate();
    }

    public BigInteger UncheckedAdd(BigInteger value)
    {
        int i = 0;
        sbyte carry = 0;
        while (true)
        {
            Digits[i] += value.Digits[i];
            Digits[i] += carry;
            if (Digits[i] >= NUM_BASE)
            {
                carry = 1;
                Digits[i] -= NUM_BASE;
            }
            else
            {
                carry = 0;
            }

            i++;
            if (i >= NumDigits && i >= value.NumDigits)
            {
                if (carry > 0) Digits[i++] = carry;
                break;
            }
        }

        NumDigits = i;
        return this;
    }

    public BigInteger UncheckedSubtract(BigInteger value)
    {
        int i = 0;
        sbyte borrow = 0;
        while (true)
        {
            if (Digits[i] < value.Digits[i] + borrow)
            {
                Digits[i] += NUM_BASE;
                Digits[i] -= value.Digits[i];
                Digits[i] -= borrow;
                borrow = 1;
            }
            else
            {
                Digits[i] -= value.Digits[i];
                Digits[i] -= borrow;
                borrow = 0;
            }

            i++;
            if (i >= NumDigits && i >= value.NumDigits) break;
        }

        for (i = NumDigits - 1; i >= 0; i--)
            if (Digits[i] != 0)
                break;
        NumDigits = i + 1;
        return this;
    }

    public BigInteger UncheckedAddNeg(BigInteger value)
    {
        int i = 0;
        sbyte borrow = 0;
        while (true)
        {
            Digits[i] -= value.Digits[i];
            Digits[i] += borrow;
            if (Digits[i] > 0)
            {
                Digits[i] = (sbyte) -(Digits[i] - NUM_BASE);
                borrow = 1;
            }
            else
            {
                Digits[i] = (sbyte) -Digits[i];
                borrow = 0;
            }

            i++;
            if (i >= value.NumDigits) break;
        }

        for (i = value.NumDigits - 1; i >= 0; i--)
            if (Digits[i] != 0)
                break;
        NumDigits = i + 1;
        return this;
    }

    public BigInteger Add(BigInteger value)
    {
        if (NumDigits > value.NumDigits)
            for (int i = NumDigits - 1; i >= value.NumDigits; i--)
                value.Digits[i] = 0;
        else if (value.NumDigits > NumDigits)
            for (int i = value.NumDigits - 1; i >= NumDigits; i--)
                Digits[i] = 0;

        if ((Sign > 0 && value.Sign > 0) || (Sign < 0 && value.Sign < 0)) return UncheckedAdd(value);
        int m = CompareMagnitude(this, value);
        if (m == 0) Zero();
        if (Sign > 0 && value.Sign < 0)
        {
            if (m == -1)
            {
                UncheckedSubtract(value);
            }
            else if (m == 1)
            {
                UncheckedAddNeg(value);
                Sign = -1;
            }
        }
        else if (Sign < 0 && value.Sign > 0)
        {
            if (m == -1)
            {
                UncheckedSubtract(value);
            }
            else if (m == 1)
            {
                UncheckedAddNeg(value);
                Sign = 1;
            }
        }

        return this;
    }


    private static int CompareMagnitude(BigInteger left, BigInteger right)
    {
        if (left.NumDigits < right.NumDigits) return 1;

        if (left.NumDigits > right.NumDigits) return -1;
        for (int i = left.NumDigits - 1; i >= 0; i--)
        {
            if (left.Digits[i] < right.Digits[i]) return 1;

            if (left.Digits[i] > right.Digits[i]) return -1;
        }

        return 0;
    }

    public BigInteger UncheckedMultiply(BigInteger value)
    {
        int numTempDigits, i, newNumDigits = 0;
        sbyte carry = 0;
        Array.Clear(mulTemp2, 0, value.NumDigits + NumDigits + 2);

        for (int d = 0; d < value.NumDigits; d++)
        {
            numTempDigits = 0;
            for (i = 0; i < NumDigits; i++)
            {
                mulTemp1[numTempDigits] = (sbyte) (value.Digits[d] * Digits[i] + carry);
                if (mulTemp1[numTempDigits] >= NUM_BASE)
                {
                    carry = (sbyte) (mulTemp1[numTempDigits] / NUM_BASE);
                    mulTemp1[numTempDigits] %= NUM_BASE;
                }
                else
                {
                    carry = 0;
                }

                numTempDigits++;
            }

            if (carry > 0) mulTemp1[numTempDigits++] = carry;

            i = 0;
            carry = 0;
            while (true)
            {
                mulTemp2[i + d] += mulTemp1[i];
                mulTemp2[i + d] += carry;
                if (mulTemp2[i + d] >= NUM_BASE)
                {
                    carry = 1;
                    mulTemp2[i + d] -= NUM_BASE;
                }
                else
                {
                    carry = 0;
                }

                i++;
                if (i >= numTempDigits)
                {
                    if (carry > 0) mulTemp2[i++ + d] = carry;
                    break;
                }
            }

            newNumDigits = i + d;
        }

        NumDigits = newNumDigits;
        for (i = NumDigits - 1; i >= 0; i--)
            if (mulTemp2[i] != 0)
                break;
        NumDigits = i + 1;
        Array.Copy(mulTemp2, Digits, NumDigits);
        return this;
    }

    public BigInteger Multiply(BigInteger value)
    {
        if (IsZero() || value.IsZero())
        {
            Zero();
            return this;
        }

        UncheckedMultiply(value);
        if (Sign == -1 && value.Sign == -1)
            Sign = 1;
        else if (Sign == 1 && value.Sign == -1) Sign = -1;
        return this;
    }


    public BigInteger Divide(BigInteger value)
    {
        if (value > this)
        {
            Zero();
            return this;
        }

        if (value.IsIntEqual(1)) return this;
        BigInteger tempVal = new BigInteger(this);
        int maxMagnitude = NumDigits - value.NumDigits;
        int i;
        Zero();
        for (int m = maxMagnitude; m >= 0; m--)
        {
            BigInteger mtest = new BigInteger(value).MulByPow10(m);

            for (i = 1; i <= NUM_BASE; i++)
            {
                BigInteger b = new BigInteger(mtest).UncheckedMultiply(i);
                if (b > tempVal) break;
            }

            i--;
            tempVal.Add(new BigInteger(mtest).Multiply(i).Negate());

            MulByPow10(1);
            Digits[0] = (sbyte) i;
        }

        return this;
    }


    public static BigInteger operator ++(BigInteger value)
    {
        return value.Add(1);
    }

    public static BigInteger operator --(BigInteger value)
    {
        return value.Add(-1);
    }

    public static BigInteger operator +(BigInteger left, BigInteger right)
    {
        return new BigInteger(left).Add(right);
    }

    public static BigInteger operator -(BigInteger left, BigInteger right)
    {
        return new BigInteger(right).Negate().Add(left);
    }

    public static BigInteger operator *(BigInteger left, BigInteger right)
    {
        return new BigInteger(left).Multiply(right);
    }


    public static bool operator ==(BigInteger left, BigInteger right)
    {
        if (left.Sign != right.Sign) return false;
        return CompareMagnitude(left, right) == 0;
    }

    public static bool operator !=(BigInteger left, BigInteger right)
    {
        return !(left == right);
    }

    public static bool operator <(BigInteger left, BigInteger right)
    {
        if (left.Sign < 0 && right.Sign > 0) return true;

        if (left.Sign > 0 && right.Sign < 0) return false;
        bool bothNegative = left.Sign < 0 && right.Sign < 0;
        int m = CompareMagnitude(left, right);
        if (m == -1) return bothNegative ? true : false;

        if (m == 1) return bothNegative ? false : true;

        return false;
    }

    public static bool operator <=(BigInteger left, BigInteger right)
    {
        if (left.Sign < 0 && right.Sign > 0) return true;

        if (left.Sign > 0 && right.Sign < 0) return false;
        bool bothNegative = left.Sign < 0 && right.Sign < 0;
        int m = CompareMagnitude(left, right);
        if (m == -1) return bothNegative ? true : false;

        if (m == 1) return bothNegative ? false : true;

        return true;
    }

    public static bool operator >(BigInteger left, BigInteger right)
    {
        return !(left <= right);
    }

    public static bool operator >=(BigInteger left, BigInteger right)
    {
        return !(left < right);
    }


    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(NumDigits + 2);
        if (Sign < 0) sb.Append("-");
        for (int i = NumDigits - 1; i >= 0; i--) sb.Append(Digits[i]);
        return sb.ToString();
    }

    public bool Equals(BigInteger other)
    {
        return this == other;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is BigInteger && Equals((BigInteger) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = Sign;
            for (int i = 0; i < NumDigits; i++) hashCode += Digits[i];
            return hashCode;
        }
    }
}