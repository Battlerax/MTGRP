
declare namespace System {

    export class Array<T> {
        Length: number;
        LongLength: number;
        Rank: number;
        // System.Collections.ICollection.Count: number;
        Count: number;
        SyncRoot: any;
        IsReadOnly: boolean;
        IsFixedSize: boolean;
        IsSynchronized: boolean;
        // System.Collections.IList.Item: any;
        Item: any;
        // static AsReadOnly(array: T[]): System.Collections.ObjectModel.ReadOnlyCollection<T>;
        // static Resize(array: T[]&, newSize: number): void;
        // static CreateInstance(elementType: System.Type, length: number): System.Array;
        // static CreateInstance(elementType: System.Type, length1: number, length2: number): System.Array;
        // static CreateInstance(elementType: System.Type, length1: number, length2: number, length3: number): System.Array;
        // static CreateInstance(elementType: System.Type, ...lengths: number[]): System.Array;
        // static CreateInstance(elementType: System.Type, ...lengths: number[]): System.Array;
        // static CreateInstance(elementType: System.Type, lengths: number[], lowerBounds: number[]): System.Array;
        // static Copy(sourceArray: System.Array, destinationArray: System.Array, length: number): void;
        // static Copy(sourceArray: System.Array, sourceIndex: number, destinationArray: System.Array, destinationIndex: number, length: number): void;
        // static ConstrainedCopy(sourceArray: System.Array, sourceIndex: number, destinationArray: System.Array, destinationIndex: number, length: number): void;
        // static Copy(sourceArray: System.Array, destinationArray: System.Array, length: number): void;
        // static Copy(sourceArray: System.Array, sourceIndex: number, destinationArray: System.Array, destinationIndex: number, length: number): void;
        // static Clear(array: System.Array, index: number, length: number): void;
        GetValue(...indices: number[]): any;
        GetValue(index: number): any;
        GetValue(index1: number, index2: number): any;
        GetValue(index1: number, index2: number, index3: number): any;
        GetValue(index: number): any;
        GetValue(index1: number, index2: number): any;
        GetValue(index1: number, index2: number, index3: number): any;
        GetValue(...indices: number[]): any;
        SetValue(value: any, index: number): void;
        SetValue(value: any, index1: number, index2: number): void;
        SetValue(value: any, index1: number, index2: number, index3: number): void;
        SetValue(value: any, ...indices: number[]): void;
        SetValue(value: any, index: number): void;
        SetValue(value: any, index1: number, index2: number): void;
        SetValue(value: any, index1: number, index2: number, index3: number): void;
        SetValue(value: any, ...indices: number[]): void;
        GetLength(dimension: number): number;
        GetLongLength(dimension: number): number;
        GetUpperBound(dimension: number): number;
        GetLowerBound(dimension: number): number;
        Clone(): any;
        // static BinarySearch(array: System.Array, value: any): number;
        // static BinarySearch(array: System.Array, index: number, length: number, value: any): number;
        // static BinarySearch(array: System.Array, value: any, comparer: System.Collections.IComparer): number;
        // static BinarySearch(array: System.Array, index: number, length: number, value: any, comparer: System.Collections.IComparer): number;
        // static BinarySearch(array: T[], value: T): number;
        // static BinarySearch(array: T[], value: T, comparer: T): number;
        // static BinarySearch(array: T[], index: number, length: number, value: T): number;
        // static BinarySearch(array: T[], index: number, length: number, value: T, comparer: T): number;
        // static ConvertAll(array: TInput[], converter: TInput): TOutput[];
        CopyTo(array: System.Array<T>, index: number): void;
        CopyTo(array: System.Array<T>, index: number): void;
        // static Empty(): T[];
        // static Exists(array: T[], match: T): boolean;
        // static Find(array: T[], match: T): T;
        // static FindAll(array: T[], match: T): T[];
        // static FindIndex(array: T[], match: T): number;
        // static FindIndex(array: T[], startIndex: number, match: T): number;
        // static FindIndex(array: T[], startIndex: number, count: number, match: T): number;
        // static FindLast(array: T[], match: T): T;
        // static FindLastIndex(array: T[], match: T): number;
        // static FindLastIndex(array: T[], startIndex: number, match: T): number;
        // static FindLastIndex(array: T[], startIndex: number, count: number, match: T): number;
        // static ForEach(array: T[], action: T): void;
        // GetEnumerator(): System.Collections.IEnumerator;
        // static IndexOf(array: System.Array, value: any): number;
        // static IndexOf(array: System.Array, value: any, startIndex: number): number;
        // static IndexOf(array: System.Array, value: any, startIndex: number, count: number): number;
        // static IndexOf(array: T[], value: T): number;
        // static IndexOf(array: T[], value: T, startIndex: number): number;
        // static IndexOf(array: T[], value: T, startIndex: number, count: number): number;
        // static LastIndexOf(array: System.Array, value: any): number;
        // static LastIndexOf(array: System.Array, value: any, startIndex: number): number;
        // static LastIndexOf(array: System.Array, value: any, startIndex: number, count: number): number;
        // static LastIndexOf(array: T[], value: T): number;
        // static LastIndexOf(array: T[], value: T, startIndex: number): number;
        // static LastIndexOf(array: T[], value: T, startIndex: number, count: number): number;
        // static Reverse(array: System.Array): void;
        // static Reverse(array: System.Array, index: number, length: number): void;
        // static Sort(array: System.Array): void;
        // static Sort(keys: System.Array, items: System.Array): void;
        // static Sort(array: System.Array, index: number, length: number): void;
        // static Sort(keys: System.Array, items: System.Array, index: number, length: number): void;
        // static Sort(array: System.Array, comparer: System.Collections.IComparer): void;
        // static Sort(keys: System.Array, items: System.Array, comparer: System.Collections.IComparer): void;
        // static Sort(array: System.Array, index: number, length: number, comparer: System.Collections.IComparer): void;
        // static Sort(keys: System.Array, items: System.Array, index: number, length: number, comparer: System.Collections.IComparer): void;
        // static Sort(array: T[]): void;
        // static Sort(keys: TKey[], items: TValue[]): void;
        // static Sort(array: T[], index: number, length: number): void;
        // static Sort(keys: TKey[], items: TValue[], index: number, length: number): void;
        // static Sort(array: T[], comparer: T): void;
        // static Sort(keys: TKey[], items: TValue[], comparer: TKey): void;
        // static Sort(array: T[], index: number, length: number, comparer: T): void;
        // static Sort(keys: TKey[], items: TValue[], index: number, length: number, comparer: TKey): void;
        // static Sort(array: T[], comparison: T): void;
        // static TrueForAll(array: T[], match: T): boolean;
        Initialize(): void;
        [index: number]: T;
    }

    class TimeSpan {
        TicksPerMillisecond: number;
        TicksPerSecond: number;
        TicksPerMinute: number;
        TicksPerHour: number;
        TicksPerDay: number;
        Zero: System.TimeSpan;
        MaxValue: System.TimeSpan;
        MinValue: System.TimeSpan;
        Ticks: number;
        Days: number;
        Hours: number;
        Milliseconds: number;
        Minutes: number;
        Seconds: number;
        TotalDays: number;
        TotalHours: number;
        TotalMilliseconds: number;
        TotalMinutes: number;
        TotalSeconds: number;
        constructor(ticks: number);
        constructor(hours: number, minutes: number, seconds: number);
        constructor(days: number, hours: number, minutes: number, seconds: number);
        constructor(days: number, hours: number, minutes: number, seconds: number, milliseconds: number);
        Add(ts: System.TimeSpan): System.TimeSpan;
        static Compare(t1: System.TimeSpan, t2: System.TimeSpan): number;
        CompareTo(value: any): number;
        CompareTo(value: System.TimeSpan): number;
        static FromDays(value: number): System.TimeSpan;
        Duration(): System.TimeSpan;
        Equals(value: any): boolean;
        Equals(obj: System.TimeSpan): boolean;
        static Equals(t1: System.TimeSpan, t2: System.TimeSpan): boolean;
        GetHashCode(): number;
        static FromHours(value: number): System.TimeSpan;
        static FromMilliseconds(value: number): System.TimeSpan;
        static FromMinutes(value: number): System.TimeSpan;
        Negate(): System.TimeSpan;
        static FromSeconds(value: number): System.TimeSpan;
        Subtract(ts: System.TimeSpan): System.TimeSpan;
        static FromTicks(value: number): System.TimeSpan;
        static Parse(s: string): System.TimeSpan;
        //static Parse(input: string, formatProvider: System.IFormatProvider): System.TimeSpan;
        //static ParseExact(input: string, format: string, formatProvider: System.IFormatProvider): System.TimeSpan;
        //static ParseExact(input: string, formats: string[], formatProvider: System.IFormatProvider): System.TimeSpan;
        //static ParseExact(input: string, format: string, formatProvider: System.IFormatProvider, styles: System.Globalization.TimeSpanStyles): System.TimeSpan;
        //static ParseExact(input: string, formats: string[], formatProvider: System.IFormatProvider, styles: System.Globalization.TimeSpanStyles): System.TimeSpan;
        //static TryParse(s: string, result: System.TimeSpan &): boolean;
        //static TryParse(input: string, formatProvider: System.IFormatProvider, result: System.TimeSpan &): boolean;
        //static TryParseExact(input: string, format: string, formatProvider: System.IFormatProvider, result: System.TimeSpan &): boolean;
        //static TryParseExact(input: string, formats: string[], formatProvider: System.IFormatProvider, result: System.TimeSpan &): boolean;
        //static TryParseExact(input: string, format: string, formatProvider: System.IFormatProvider, styles: System.Globalization.TimeSpanStyles, result: System.TimeSpan &): boolean;
        //static TryParseExact(input: string, formats: string[], formatProvider: System.IFormatProvider, styles: System.Globalization.TimeSpanStyles, result: System.TimeSpan &): boolean;
        ToString(): string;
        ToString(format: string): string;
        //ToString(format: string, formatProvider: System.IFormatProvider): string;
    }

    interface IAsyncResult {
        IsCompleted: boolean;
        //AsyncWaitHandle: System.Threading.WaitHandle;
        AsyncWaitHandle: any;
        AsyncState: any;
        CompletedSynchronously: boolean;
    }

    class AsyncCallback {
        constructor(object: any, method: number);
        Invoke(ar: System.IAsyncResult): void;
        BeginInvoke(ar: System.IAsyncResult, callback: System.AsyncCallback, object: any): System.IAsyncResult;
        EndInvoke(result: System.IAsyncResult): void;
    }

    interface EventArgs {
    }

    namespace Windows {
        namespace Forms {
            class KeyEventArgs {
                Alt: boolean;
                Control: boolean;
                Handled: boolean;
                KeyCode: System.Windows.Forms.Keys;
                KeyValue: number;
                KeyData: System.Windows.Forms.Keys;
                Modifiers: System.Windows.Forms.Keys;
                Shift: boolean;
                SuppressKeyPress: boolean;
                constructor(keyData: System.Windows.Forms.Keys);
            }

            enum Keys {
                None = 0,
                Cancel = 1,
                Back = 2,
                Tab = 3,
                LineFeed = 4,
                Clear = 5,
                Enter = 6,
                Return = 6,
                Pause = 7,
                Capital = 8,
                CapsLock = 8,
                HangulMode = 9,
                KanaMode = 9,
                JunjaMode = 10,
                FinalMode = 11,
                HanjaMode = 12,
                KanjiMode = 12,
                Escape = 13,
                ImeConvert = 14,
                ImeNonConvert = 15,
                ImeAccept = 16,
                ImeModeChange = 17,
                Space = 18,
                PageUp = 19,
                Prior = 19,
                Next = 20,
                PageDown = 20,
                End = 21,
                Home = 22,
                Left = 23,
                Up = 24,
                Right = 25,
                Down = 26,
                Select = 27,
                Print = 28,
                Execute = 29,
                PrintScreen = 30,
                Snapshot = 30,
                Insert = 31,
                Delete = 32,
                Help = 33,
                D0 = 34,
                D1 = 35,
                D2 = 36,
                D3 = 37,
                D4 = 38,
                D5 = 39,
                D6 = 40,
                D7 = 41,
                D8 = 42,
                D9 = 43,
                A = 44,
                B = 45,
                C = 46,
                D = 47,
                E = 48,
                F = 49,
                G = 50,
                H = 51,
                I = 52,
                J = 53,
                K = 54,
                L = 55,
                M = 56,
                N = 57,
                O = 58,
                P = 59,
                Q = 60,
                R = 61,
                S = 62,
                T = 63,
                U = 64,
                V = 65,
                W = 66,
                X = 67,
                Y = 68,
                Z = 69,
                LWin = 70,
                RWin = 71,
                Apps = 72,
                Sleep = 73,
                NumPad0 = 74,
                NumPad1 = 75,
                NumPad2 = 76,
                NumPad3 = 77,
                NumPad4 = 78,
                NumPad5 = 79,
                NumPad6 = 80,
                NumPad7 = 81,
                NumPad8 = 82,
                NumPad9 = 83,
                Multiply = 84,
                Add = 85,
                Separator = 86,
                Subtract = 87,
                Decimal = 88,
                Divide = 89,
                F1 = 90,
                F2 = 91,
                F3 = 92,
                F4 = 93,
                F5 = 94,
                F6 = 95,
                F7 = 96,
                F8 = 97,
                F9 = 98,
                F10 = 99,
                F11 = 100,
                F12 = 101,
                F13 = 102,
                F14 = 103,
                F15 = 104,
                F16 = 105,
                F17 = 106,
                F18 = 107,
                F19 = 108,
                F20 = 109,
                F21 = 110,
                F22 = 111,
                F23 = 112,
                F24 = 113,
                NumLock = 114,
                Scroll = 115,
                LeftShift = 116,
                RightShift = 117,
                LeftCtrl = 118,
                RightCtrl = 119,
                LeftAlt = 120,
                RightAlt = 121,
                BrowserBack = 122,
                BrowserForward = 123,
                BrowserRefresh = 124,
                BrowserStop = 125,
                BrowserSearch = 126,
                BrowserFavorites = 127,
                BrowserHome = 128,
                VolumeMute = 129,
                VolumeDown = 130,
                VolumeUp = 131,
                MediaNextTrack = 132,
                MediaPreviousTrack = 133,
                MediaStop = 134,
                MediaPlayPause = 135,
                LaunchMail = 136,
                SelectMedia = 137,
                LaunchApplication1 = 138,
                LaunchApplication2 = 139,
                Oem1 = 140,
                OemSemicolon = 140,
                OemPlus = 141,
                OemComma = 142,
                OemMinus = 143,
                OemPeriod = 144,
                Oem2 = 145,
                OemQuestion = 145,
                Oem3 = 146,
                OemTilde = 146,
                AbntC1 = 147,
                AbntC2 = 148,
                Oem4 = 149,
                OemOpenBrackets = 149,
                Oem5 = 150,
                OemPipe = 150,
                Oem6 = 151,
                OemCloseBrackets = 151,
                Oem7 = 152,
                OemQuotes = 152,
                Oem8 = 153,
                Oem102 = 154,
                OemBackslash = 154,
                ImeProcessed = 155,
                System = 156,
                DbeAlphanumeric = 157,
                OemAttn = 157,
                DbeKatakana = 158,
                OemFinish = 158,
                DbeHiragana = 159,
                OemCopy = 159,
                DbeSbcsChar = 160,
                OemAuto = 160,
                DbeDbcsChar = 161,
                OemEnlw = 161,
                DbeRoman = 162,
                OemBackTab = 162,
                Attn = 163,
                DbeNoRoman = 163,
                CrSel = 164,
                DbeEnterWordRegisterMode = 164,
                DbeEnterImeConfigureMode = 165,
                ExSel = 165,
                DbeFlushString = 166,
                EraseEof = 166,
                DbeCodeInput = 167,
                Play = 167,
                DbeNoCodeInput = 168,
                Zoom = 168,
                DbeDetermineString = 169,
                NoName = 169,
                DbeEnterDialogConversionMode = 170,
                Pa1 = 170,
                OemClear = 171,
                DeadCharProcessed = 172,
            }
        }
    }
}
