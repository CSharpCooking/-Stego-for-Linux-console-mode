using System;

namespace Stego
{
    public class WrongSize : Exception
    { }

    class Stegomask
    {
        int n_kontur = 9 * 60 - 12;
        int m = 60;
        int[,] _elem; // эталоны
        int[,] _sohr_el; // ключи 

        public Stegomask(int size_object)
        {
            if (size_object < 11) throw new WrongSize();
            n_kontur = 9 * size_object - 12;
            m = size_object;
            _sohr_el = new int[m + 1, n_kontur]; // ключи по контуру
            _elem = new int[m + 1, n_kontur]; // эталоны по контуру
            int[,] mas = new int[m, m * 2 - 1]; // буфер, используемый при генерации эталонов по контуру

            for (int j = 0; j < m * 2 - 1; j++)
                if (j == 0 || j == m * 2 - 2) for (int i = 0; i < m; i++) mas[i, j] = 1;
                else
                {
                    mas[0, j] = 1;
                    for (int k = 1; k < m - 1; k++) mas[k, j] = 0;
                    mas[m - 1, j] = 1;
                }
            konturGen(mas, 10);

            for (int j = 0; j < m * 2 - 1; j++)
            {
                for (int k = 0; k < m - 1; k++)
                    if (k + 1 == m - j) mas[k, j] = 1;
                    else mas[k, j] = 0;
            }
            konturGen(mas, 1);

            for (int j = 0; j < m * 2 - 1; j++)
                if (j < m || j == m * 2 - 2)
                {
                    if (j == 0 || j == m * 2 - 2) for (int i = 0; i < m; i++) mas[i, j] = 1;
                    else
                    {
                        mas[0, j] = 1;
                        for (int k = 1; k < m - 1; k++) mas[k, j] = 0;
                        mas[m - 1, j] = 1;
                    }
                }
                else
                {
                    int k;
                    for (k = 0; k < m - 1; k++)
                        if (k + 2 == 2 * m - j) mas[k, j] = 1;
                        else mas[k, j] = 0;
                    mas[k, j] = 0;
                }
            konturGen(mas, 2);

            for (int j = 0; j < m * 2 - 1; j++)
            {
                int i = 0;
                if (j == 0 || j == m - 1) for (i = 0; i < m; i++) mas[i, j] = 1;
                else
                    if (j == m * 2 - 2) { mas[i, j] = 1; for (i = 1; i < m; i++) mas[i, j] = 0; }
                    else
                    {
                        int k;
                        for (k = 0; k < m - 1; k++)
                            if (k + 2 == 2 * m - j || k + 1 == m - j) mas[k, j] = 1;
                            else mas[k, j] = 0;
                        mas[k, j] = 0;
                    }
            }
            konturGen(mas, 3);

            for (int j = 0; j < m * 2 - 1; j++)
            {
                if (j < m - 1)
                {
                    mas[0, j] = 1;
                    for (int k = 1; k < m - 1; k++) mas[k, j] = 0;
                    mas[m - 1, j] = 1;
                }
                if (j == m - 1) for (int i = 0; i < m; i++) mas[i, j] = 1;
                if (j > m - 1)
                {
                    int k = 0;
                    for (; k < m - 1; k++) mas[k, j] = 0;
                    mas[k, j] = 1;
                }
            }
            konturGen(mas, 4);

            for (int j = 0; j < m * 2 - 1; j++)
            {
                int k = 0;
                if (j == 0 || j == m * 2 - 2 || j == m - 1) for (k = 0; k < m; k++) mas[k, j] = 1;
                else
                {
                    if (j < m - 1)
                    {
                        mas[0, j] = 1;
                        for (k = 1; k < m; k++) mas[k, j] = 0;
                    }
                    if (j > m - 1)
                    {
                        for (k = 0; k < m - 1; k++) mas[k, j] = 0;
                        mas[k, j] = 1;
                    }
                }
            }
            konturGen(mas, 5);

            for (int j = 0; j < m * 2 - 1; j++)
            {
                if (j < m - 1)
                    for (int k = 0; k < m; k++)
                        if (k + 1 == m - j) mas[k, j] = 1;
                        else mas[k, j] = 0;
                if (j == m - 1 || j == 2 * m - 2) for (int i = 0; i < m; i++) mas[i, j] = 1;
                else
                    if (j > m - 1)
                    {
                        int k;
                        mas[0, j] = 1;
                        for (k = 1; k < m - 1; k++) mas[k, j] = 0;
                        mas[k, j] = 1;
                    }
            }
            konturGen(mas, 6);

            for (int j = 0; j < m * 2 - 1; j++)
            {
                if (j == 0) for (int i = 0; i < m; i++) mas[i, j] = 1;
                else
                    if (j < m - 1)
                    {
                        int k;
                        for (k = 0; k < m - 1; k++)
                            if (k + 1 == m - j) mas[k, j] = 1;
                            else mas[k, j] = 0;
                        mas[k, j] = 0;
                    }
                if (j > m - 2)
                {
                    mas[0, j] = 1;
                    for (int k = 1; k < m; k++) mas[k, j] = 0;
                }
            }
            konturGen(mas, 7);

            for (int j = 0; j < m * 2 - 1; j++)
            {
                if (j == 0 || j == m - 1 || j == 2 * m - 2) for (int i = 0; i < m; i++) mas[i, j] = 1;
                else
                {
                    int k;
                    mas[0, j] = 1;
                    for (k = 1; k < m - 1; k++) mas[k, j] = 0;
                    mas[k, j] = 1;
                }
            }
            konturGen(mas, 8);

            for (int j = 0; j < m * 2 - 1; j++)
            {
                if (j == 0 || j == m - 1) for (int i = 0; i < m; i++) mas[i, j] = 1;
                else
                    if (j < m - 1)
                    {
                        int k;
                        mas[0, j] = 1;
                        for (k = 1; k < m - 1; k++) mas[k, j] = 0;
                        mas[k, j] = 1;
                    }
                if (j > m - 1)
                {
                    int k;
                    for (k = 0; k < m - 1; k++)
                        if (k + 2 == 2 * m - j) mas[k, j] = 1;
                        else mas[k, j] = 0;
                    mas[k, j] = 0;
                }
            }
            konturGen(mas, 9);

        }

        void konturGen(int[,] mas, int e_num)
        {
            int X = m, Y = m * 2 - 1; //размеры сетки
            int k = 0, i = 0, j = 0;

            //1 контур
            for (i = Y - 1; i >= 0; i--)
                _elem[e_num, k++] = mas[0, i];
            //2 контур
            for (i = 1; i < X; i++)
                _elem[e_num, k++] = mas[i, 0];
            //3 контур
            for (i = 1; i < Y; i++)
                _elem[e_num, k++] = mas[X - 1, i];
            //4 контур
            for (i = X - 2; i > 0; i--)
                _elem[e_num, k++] = mas[i, Y - 1];
            //5 контур	
            j = 1; i = Y - 2;
            while (j != i)
            {
                _elem[e_num, k++] = mas[j, i];
                j++; i--;
            }
            //6 контур					
            for (i = X - 2; i > 0; i--)
                _elem[e_num, k++] = mas[i, j];
            //7 контур
            i = j - 1; j = 1;
            while (j != X - 1)
            {
                _elem[e_num, k++] = mas[j, i];
                j++; i--;
            }
        }

        public void GetEtalons(out char[,] etalons)
        {
            etalons = new char[10, n_kontur];
            for (int i = 0; i < n_kontur; i++)
                etalons[0, i] = (char)(_elem[10, i]+48);
            for (int j = 1; j < 10; j++)
                for (int i = 0; i < n_kontur; i++)
                    etalons[j, i] = (char)(_elem[j, i]+48);
        }

        public void GetKey(out char[,] key)
        {
            key = new char[10, n_kontur];
            for (int i = 0; i < n_kontur; i++)
                key[0, i] = (char)(_sohr_el[10, i]+48);
            for (int j = 1; j < 10; j++)
                for (int i = 0; i < n_kontur; i++)
                    key[j, i] = (char)(_sohr_el[j, i]+48);
        }

        public void GenerateKey() // _sohr_el - маски
        {
            int[,] C = new int[10, m + 1];
            int[,] D = new int[10, m + 1];
            int[,] pr_otm = new int[10, m + 1];
            int[,] otm = new int[10, m + 1];
            int[] i = new int[m + 1];
            int[] gamma = new int[m + 1];
            int[] _A1 = new int[n_kontur];
            int[] _A2 = new int[n_kontur];
            int[] _A3 = new int[n_kontur];

            Random rand = new Random(DateTime.Now.Millisecond);

            int p, q, s, j, k, l, kol_ed, pp, flag;

            gamma[0] = 10;

            for (s = 1; s < gamma[0] + 1; s++)
                D[0, s] = s;

            for (s = 0; s < 10; s++) /*obnulenie massivov otmetok*/
                for (p = 1; p <= gamma[0] + 1; p++)
                { otm[s, p] = 0; pr_otm[s, p] = 0; }

            for (p = 0; p <= gamma[0]; p++)/*obnulenie massivov masok*/
                for (s = 0; s < n_kontur; s++)
                    _sohr_el[p, s] = 0;

            l = 0; /*nulevoy na4. yroven*/

            m1: s = 0;
            do
            {
                p = rand.Next() % gamma[l];
                if (p == 0) p = gamma[l];
                flag = 0;
                if (s != 0)
                    for (q = 1; q <= s; q++)
                        if (p == C[l, q]) flag = 1;
                if (flag == 0)
                { s++; C[l, s] = p; }
            } while (s < gamma[l]);
            C[l, gamma[l] + 1] = -1;

            /*3p*/
            i[l] = 1;
        m2:  /*4p*/
            j = i[l];
            k = 1;
            D[l + 1, 1] = D[l, C[l, i[l]]];
            pr_otm[l, j] = 1;
            do { j++; }
            while (otm[l, j] != 0);
            if (C[l, j] == -1)
            {
                for (s = 1; s <= gamma[l]; s++)
                { pr_otm[l, s] = 0; otm[l, s] = 0; }
            m5: l--;
                if (l >= 0)
                {
                    do { i[l]++; }
                    while (otm[l, i[l]] != 0);
                    if (C[l, i[l]] != -1) goto m2;/*k p4*/ else goto m5;
                }
                else goto m4;/*k vyv_rez*/
            }
            else
            {  /*p6*/
                for (s = 0; s < n_kontur; s++)
                {
                    _A1[s] = _elem[D[l, C[l, i[l]]], s] + _elem[D[l, C[l, j]], s];
                    if (_A1[s] == 2) _A1[s] = 0;
                }
            m3: do { j++; }	/**p7*/
                while (otm[l, j] != 0);
                if (C[l, j] != -1)
                {
                    for (s = 0; s < n_kontur; s++)
                    {
                        _A2[s] = _elem[D[l, C[l, i[l]]], s] + _elem[D[l, C[l, j]], s];
                        if (_A2[s] == 2) _A2[s] = 0;
                        _A3[s] = _A1[s];
                    }
                    flag = 0;
                    for (s = 0; s < n_kontur; s++)
                    {
                        _A1[s] = (_A1[s] > 0 ? 1 : 0) & (_A2[s] > 0 ? 1 : 0);
                        if (_A1[s] > 0) flag = 1;
                    }
                    if (flag == 0)
                    {
                        k++;
                        D[l + 1, k] = D[l, C[l, j]];
                        pr_otm[l, j] = 1;
                        for (s = 0; s < n_kontur; s++)
                            _A1[s] = _A3[s];
                    }
                    goto m3; /*k p7*/
                }
                else		/*p13*/
                {
                    kol_ed = 0; /*opredeljaem kol-vo edinic*/
                    for (p = 0; p < n_kontur; p++)
                        if (_A1[p] > 0) kol_ed++;
                    /*----------------------------------------*/
                    s = rand.Next() % kol_ed;
                    /*----------------------------------------*/
                    kol_ed = 0; /*naxodim koordinaty p q*/
                    for (pp = 0; pp < n_kontur; pp++)
                        if (_A1[pp] > 0)
                        {
                            if (s == kol_ed) { p = pp; pp = n_kontur; }
                            kol_ed++;
                        }
                    /*****************************************/

                    for (s = 1; s <= gamma[l]; s++)
                    {
                        if (otm[l, s] != 1)
                            _sohr_el[D[l, C[l, s]], p] = 1;
                        otm[l, s] = pr_otm[l, s];
                    }
                    l++;
                    if (k > 10) goto end_m;
                    gamma[l] = k;
                    goto m1; /* k p2*/
                }
            }
        m4: ;
        end_m: ;
        }
    }
}