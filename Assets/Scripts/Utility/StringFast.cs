using UnityEngine;
using System.Collections.Generic;

//TODO: Currently unused. Remove this TODO when used.

///<summary>
/// Mutable String class, optimized for speed and memory allocations while retrieving the final result as a string.
/// Similar use than StringBuilder, but avoid a lot of allocations done by StringBuilder (conversion of int and float to string, frequent capacity change, etc.)
/// Author: Nicolas Gadenne contact@gaddygames.com
///</summary>
public class StringFast {
    private readonly char[] IntToChar = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

    ///<summary>Immutable string. Generated at last moment, only if needed</summary>
    private string m_stringGenerated = "";
    ///<summary>Is m_stringGenerated is up to date ?</summary>
    private bool m_isStringGenerated = false;

    ///<summary>Working mutable string</summary>
    private char[] m_chars = null;
    private int m_charsCount = 0;
    private int m_charsCapacity = 0;

    ///<summary>Temporary string used for the Replace method</summary>
    private List<char> m_replacement = null;

    private object m_valueControl = null;
    private int m_valueControlInt = int.MinValue;

    // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

    public StringFast(int initialCapacity = 32) {
        m_chars = new char[m_charsCapacity = initialCapacity];
    }

    public bool IsEmpty() {
        return (m_isStringGenerated ? (m_stringGenerated == null) : (m_charsCount == 0));
    }

    ///<summary>Return the string</summary>
    public override string ToString() {
        if (!m_isStringGenerated) // Regenerate the immutable string if needed
        {
            m_stringGenerated = new string(m_chars, 0, m_charsCount);
            m_isStringGenerated = true;
        }
        return m_stringGenerated;
    }

    // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    // Value controls methods: use a value to check if the string has to be regenerated.

    ///<summary>Return true if the valueControl has changed (and update it)</summary>
    public bool IsModified(int newControlValue) {
        bool changed = (newControlValue != m_valueControlInt);
        if (changed)
            m_valueControlInt = newControlValue;
        return changed;
    }

    ///<summary>Return true if the valueControl has changed (and update it)</summary>
    public bool IsModified(object newControlValue) {
        bool changed = !(newControlValue.Equals(m_valueControl));
        if (changed)
            m_valueControl = newControlValue;
        return changed;
    }


    // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    // Set methods:

    public void Set(string str) {
        Clear();
        Append(str);
    }
    ///<summary>Caution, allocate some memory</summary>
    public void Set(object str) {
        Set(str.ToString());
    }
    public void Set<T1, T2>(T1 str1, T2 str2) {
        Clear();
        Append(str1); Append(str2);
    }
    public void Set<T1, T2, T3>(T1 str1, T2 str2, T3 str3) {
        Clear();
        Append(str1); Append(str2); Append(str3);
    }
    public void Set<T1, T2, T3, T4>(T1 str1, T2 str2, T3 str3, T4 str4) {
        Clear();
        Append(str1); Append(str2); Append(str3); Append(str4);
    }
    ///<summary>Allocate a little memory (20 byte)</summary>
    public void Set(params object[] str) {
        Clear();
        for (int i = 0; i < str.Length; i++)
            Append(str[i]);
    }

    // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    // Append methods, to build the string without allocation

    public StringFast Clear() {
        m_charsCount = 0;
        m_isStringGenerated = false;
        return this;
    }

    ///<summary>Append a string without memory allocation</summary>
    public StringFast Append(string value) {
        ReallocateIFN(value.Length);
        int n = value.Length;
        for (int i = 0; i < n; i++)
            m_chars[m_charsCount + i] = value[i];
        m_charsCount += n;
        m_isStringGenerated = false;
        return this;
    }
    ///<summary>Append an object.ToString(), allocate some memory</summary>
    public StringFast Append(object value) {
        Append(value.ToString());
        return this;
    }

    ///<summary>Append an int without memory allocation</summary>
    public StringFast Append(int value) {
        // Allocate enough memory to handle any int number
        ReallocateIFN(16);

        // Handle the negative case
        if (value < 0) {
            value = -value;
            m_chars[m_charsCount++] = '-';
        }

        // Copy the digits in reverse order
        int nbChars = 0;
        do {
            m_chars[m_charsCount++] = IntToChar[value % 10];
            value /= 10;
            nbChars++;
        } while (value != 0);

        // Reverse the result
        for (int i = nbChars / 2 - 1; i >= 0; i--) {
            char c = m_chars[m_charsCount - i - 1];
            m_chars[m_charsCount - i - 1] = m_chars[m_charsCount - nbChars + i];
            m_chars[m_charsCount - nbChars + i] = c;
        }
        m_isStringGenerated = false;
        return this;
    }

    ///<summary>Append a float without memory allocation</summary>
    public StringFast Append(float value) {
        // Allocate enough memory to handle any float number
        ReallocateIFN(16);

        // Handle the negative case
        if (value < 0) {
            value = -value;
            m_chars[m_charsCount++] = '-';
        }

        // Transform the float into an int and get the number of floating digits
        int nbFloatDigits = 0;
        while (Mathf.Abs(value - Mathf.Round(value)) > 0.00001f) {
            value *= 10;
            nbFloatDigits++;
        }
        int valueInt = Mathf.RoundToInt(value);

        // Copy the digits in reverse order
        int nbChars = 0;
        do {
            m_chars[m_charsCount++] = IntToChar[valueInt % 10];
            valueInt /= 10;
            nbChars++;
            if (nbFloatDigits == nbChars) // Add the point
            {
                m_chars[m_charsCount++] = '.';
                nbChars++;
            }
        } while (valueInt != 0 || nbChars <= nbFloatDigits + 1);

        // Reverse the result
        for (int i = nbChars / 2 - 1; i >= 0; i--) {
            char c = m_chars[m_charsCount - i - 1];
            m_chars[m_charsCount - i - 1] = m_chars[m_charsCount - nbChars + i];
            m_chars[m_charsCount - nbChars + i] = c;
        }
        m_isStringGenerated = false;
        return this;
    }

    // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

    ///<summary>Replace all occurences of a string by another one</summary>
    public StringFast Replace(string oldStr, string newStr) {
        if (m_charsCount == 0)
            return this;

        if (m_replacement == null)
            m_replacement = new List<char>();

        // Create the new string into m_replacement
        for (int i = 0; i < m_charsCount; i++) {
            bool isToReplace = false;
            if (m_chars[i] == oldStr[0]) // If first character found, check for the rest of the string to replace
            {
                int k = 1;
                while (k < oldStr.Length && m_chars[i + k] == oldStr[k])
                    k++;
                isToReplace = (k >= oldStr.Length);
            }
            if (isToReplace) // Do the replacement
            {
                i += oldStr.Length - 1;
                if (newStr != null)
                    for (int k = 0; k < newStr.Length; k++)
                        m_replacement.Add(newStr[k]);
            }
            else // No replacement, copy the old character
                m_replacement.Add(m_chars[i]);
        }

        // Copy back the new string into m_chars
        ReallocateIFN(m_replacement.Count - m_charsCount);
        for (int k = 0; k < m_replacement.Count; k++)
            m_chars[k] = m_replacement[k];
        m_charsCount = m_replacement.Count;
        m_replacement.Clear();
        m_isStringGenerated = false;
        return this;
    }

    // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

    private void ReallocateIFN(int nbCharsToAdd) {
        if (m_charsCount + nbCharsToAdd > m_charsCapacity) {
            m_charsCapacity = System.Math.Max(m_charsCapacity + nbCharsToAdd, m_charsCapacity * 2);
            char[] newChars = new char[m_charsCapacity];
            m_chars.CopyTo(newChars, 0);
            m_chars = newChars;
        }
    }

}