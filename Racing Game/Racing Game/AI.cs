using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Racing_Game
{
    class AI
    {
        // Pozycja samochodu
        private Vector2 _position;

        // Enumeracja z mozliwymi kierunkami jazdy
        private enum Dest { North, East, South, West };
        // Kierunek jazdy
        private Dest destination;

        // Zmienne okreslajace czy samochod skreca w lewo/praco
        private bool rotatefirstAiLeft = false;
        private bool rotatefirstAiRight = false;

        // Kolor obszaru trasy
        private Color reference;

        // Predkosc poczatkowa
        private float _initialVelocity;
        // Aktualna predkosc
        private Vector2 _velocity;
        // Predkosc skretu
        private const float tangentialVelocity = 3f;

        // Wartosc okreslajaca skret pojazdu
        private float _rotation;

        // Liczba przejechanych okrazen
        private int _lap = 0;
        // Flaga kontrolna - zapobiega wielokrotnej zmianie okrazenia
        private bool lapChange = false;
        // Czas ostatniej zmiany okrazenia - domyslnie 10 minut temu, aby przy pierwszym przejechaniu zmienic wartosc zmiennej
        private DateTime lastLapChange = DateTime.Now.Subtract(TimeSpan.FromMinutes(10));

        // Flaga - wystąpienie kolizji
        private bool _collision = false;

        // Odleglosc blizszych czujnikow
        private int nearerSensor = 150;
        // Odleglosc dalszych czujnikow
        private int furtherSensor = 210;

        // Ustalenie dlugosci poslizgu do zatrzymania
        private float friction = 0.1f;

        // Pozycja - pole publiczne
        public Vector2 position
        {
            get
            {
                return this._position;
            }
            set
            {
                this._position = value;
            }
        }

        // Predkosc - pole publiczne
        public Vector2 velocity
        {
            get
            {
                return this._velocity;
            }
            set
            {
                this._velocity = value;
            }
        }

        // Obrot - pole publiczne
        public float rotation
        {
            get
            {
                return this._rotation;
            }
            set
            {
                this._rotation = value;
            }
        }

        // Okrazenie - pole publiczne (tylko do odczytu)
        public int lap
        {
            get
            {
                return this._lap;
            }
        }

        // Czas ostatniej zmiany okrazenia (tylko do odczytu)
        public DateTime lapChanged
        {
            get
            {
                return this.lastLapChange;
            }
        }

        // Kolizja
        public bool collision
        {
            get
            {
                return this._collision;
            }
            set
            {
                this._collision = value;
            }
        }

        // Konstruktor
        // Parametry - pozycja poczatkowa, predkosc
        public AI(Vector2 initPos, float initXVel)
        {
            // Ustawienie pozycji poczatkowej
            this._position = initPos;
            // Ustawienie koloru tla trasy
            this.reference = new Color(114, 114, 113, 255);
            // Ustawienie predkosci
            this._velocity = new Vector2(initXVel, 0f);
            // Ustawienie predkosci poczatkowej
            this._initialVelocity = initXVel;
            // Ustawienie kierunku jazdy
            this.destination = Dest.East;
        }

        // Obliczanie pozycji
        public void CalculatePosition(Texture2D background)
        {
            // Ustawiamy kolor pod czujkami - domyslnie przezroczysty, poniewaz nie moze
            // on wystapic w rzeczywistosci, co pozwoli nam okreslic, czy zostal on faktycznie
            // zmieniony w pozniejszym etapie.
            Color[] east_n = new Color[] { Color.Transparent };
            Color[] east_f = new Color[] { Color.Transparent };
            Color[] west_n = new Color[] { Color.Transparent };
            Color[] west_f = new Color[] { Color.Transparent };
            Color[] north_n = new Color[] { Color.Transparent };
            Color[] north_f = new Color[] { Color.Transparent };
            Color[] south_n = new Color[] { Color.Transparent };
            Color[] south_f = new Color[] { Color.Transparent };

            // Jezeli nie wykonujemy skretu
            if (!rotatefirstAiLeft && !rotatefirstAiRight)
            {
                // Pobieramy kolor tla pod dana czujka. Dodatkowe 400 pikseli wystepuje
                // ze wzgledu na przesuniecie tla.
                east_n = getBackgroundColor(400 + this.nearerSensor, 0, position, background);
                east_f = getBackgroundColor(400 + this.furtherSensor, 0, position, background);
                west_n = getBackgroundColor(400 - this.nearerSensor, 0, position, background);
                west_f = getBackgroundColor(400 - this.furtherSensor, 0, position, background);
                north_n = getBackgroundColor(400, -this.nearerSensor, position, background);
                north_f = getBackgroundColor(400, -this.furtherSensor, position, background);
                south_n = getBackgroundColor(400, this.nearerSensor, position, background);
                south_f = getBackgroundColor(400, this.furtherSensor, position, background);
            }

            // Ponownie - jesli nie wykonujemy skretu
            if (!rotatefirstAiLeft && !rotatefirstAiRight)
            {
                // Tworzymy zmienne dla czujek, ktore faktycznie bedziemy chcieli sprawdzac
                // (np. przy jezdzie w kierunku wschodnim sprawdzamy, czy mamy mozliwosc skretu
                // na polnoc i polodnie). Zapisujemy poczatkowo przezroczystosc, z tych samych
                // powodow co powyzej.
                Color[] check1_n = new Color[] { Color.Transparent };
                Color[] check1_f = new Color[] { Color.Transparent };
                Color[] check2_n = new Color[] { Color.Transparent };
                Color[] check2_f = new Color[] { Color.Transparent };
                // Ustawiamy czujki w zaleznosci od kierunku jazdy i tylko, gdy pod dalsza czujka
                // dla danego kierunku znajduje sie kolor inny, niz okreslony w zmiennej reference.
                // Czujki wskazuja na mozliwosc skretu w lewo (pierwsza) lub w prawo (druga).
                // Przy kierunku jazdy na wschod...
                if (destination == Dest.East && east_n[0] != reference)
                {
                    // ...wlasciwymi czujkami sa polnocna i polodniowa.
                    check1_n = north_n;
                    check1_f = north_f;
                    check2_n = south_n;
                    check2_f = south_f;
                }
                // Przy kierunku jazdy na zachod...                
                else if (destination == Dest.West && west_n[0] != reference)
                {
                    // ...wlasciwymi czujkami sa polodniowa i polnocna.
                    check1_n = south_n;
                    check1_f = south_f;
                    check2_n = north_n;
                    check2_f = north_f;
                }
                // Przy kierunku jazdy na polnoc...
                else if (destination == Dest.North && north_n[0] != reference)
                {
                    // ...wlasciwymi czujkami sa zachodnia i wschodnia.
                    check1_n = west_n;
                    check1_f = west_f;
                    check2_n = east_n;
                    check2_f = east_f;
                }
                // Przy kierunku jazdy na poludnie...
                else if (destination == Dest.South && south_n[0] != reference)
                {
                    // ...wlasciwymi czujkami sa wschodnia i zachodnia.
                    check1_n = east_n;
                    check1_f = east_f;
                    check2_n = west_n;
                    check2_f = west_f;
                }

                // Jesli pierwsza czujka, polozona blizej auta, nie jest przezroczysta
                // (czyli odkrylismy trase)...
                if (check1_n[0] != Color.Transparent)
                {
                    // ...dokonujemy dodatkowego sprawdzenia - porownujemy blizsze czujki
                    if (check1_n[0] == check2_n[0])
                    {
                        // Jesli blizsze czujki wskazuja ten sam kolor, odwolujemy sie
                        // do czujek polozonych dalej i porownujemy je z kolorem trasy.
                        // Jesli pierwsza czujka pobrala kolor trasy...
                        if (check1_f[0] == reference)
                        {
                            // ...mamy mozliwosc skretu w lewo.
                            rotatefirstAiLeft = true;
                        }
                        // Dla drugiej analogicznie.
                        else if (check2_f[0] == reference)
                        {
                            rotatefirstAiRight = true;
                        }
                    }
                    // ...w przeciwnym wypadku czujka blizsza ma jednoznaczna wartosc i
                    // do niej mozemy sie odwolac (porownania ponizej jak w poprzednim warunku).
                    else
                    {
                        if (check1_n[0] == reference)
                        {
                            rotatefirstAiLeft = true;
                        }
                        else if (check2_n[0] == reference)
                        {
                            rotatefirstAiRight = true;
                        }
                    }
                }
            }

            // Jesli wystapila kolizja
            if (collision)
            {
                if (velocity.X > -0.5 && velocity.X < 0.5 && velocity.Y > -0.5 && velocity.Y < 0.5)
                {
                    collision = false;
                }
                else
                {
                    velocity *= 1 - friction;
                }
            }
            // Jesli odkrylismy koniecznosc skretu...
            else if (rotatefirstAiLeft || rotatefirstAiRight)
            {
                // Jesli skrecamy w lewo...
                if (rotatefirstAiLeft)
                {
                    // ...zwiekszamy wartosc zmiennej okreslajacej rotacje.
                    rotation -= (float)(0.01 * 3.14);
                }
                // Jesli skrecamy w prawo...
                else
                {
                    // ...zmniejszamy wartosc zmiennej okreslajacej rotacje.
                    rotation += (float)(0.01 * 3.14);
                }
                // Zaokraglamy wartosc zmiennej obrotu do czterech miejsc po przecinku.
                float round = (float)Math.Round(rotation, 4);
                // Obliczamy sinus i cosinus z wartosci zmiennej obrotu, rowniez z zaokragleniem.
                float sround = (float)(Math.Round(Math.Sin(round), 4));
                float cround = (float)(Math.Round(Math.Cos(round), 4));
                // W zaleznosci od kierunku jazdy i wartosci funkcji sinus/cosinus jestesmy w stanie stwierdzic,
                // czy samochod dokonal pelnego obrotu. Wykonujemy ogolne sprawdzenia - dla kierunkow wschod/zachod
                // zmienna sinus powinna przyjac wartosc 0, 1 lub -1, aby mozna bylo uznac skret za zakonczony.
                // Analogicznie zachowa sie zmienna cosinus dla kierunkow polnoc/poludnie.
                if (((destination == Dest.East || destination == Dest.West) && (sround == 0 || sround == 1 || sround == -1))
                    ||
                    ((destination == Dest.North || destination == Dest.South) && (cround == 0 || cround == 1 || cround == -1)))
                {
                    // Jesli sinus wynosi 1...
                    if (sround == 1)
                    {
                        // ...zmieniamy kierunek na poludniowy,
                        destination = Dest.South;
                        // predkosc pozioma zmieniamy na zero,
                        _velocity.X = 0;
                        // predkosc pionowa ustawiamy na wartosc poczatkowa,
                        _velocity.Y = this._initialVelocity;
                        // okreslamy stosowna wartosc obrotu, aby auto wyswietlilo sie w pelni pionowo
                        rotation = 1.57f;
                    }
                    // Analogicznie ustawiamy zmienne dla innych spodziewanych wartosci funkcji.
                    else if (sround == -1)
                    {
                        destination = Dest.North;
                        _velocity.X = 0;
                        _velocity.Y = -this._initialVelocity;
                        rotation = 4.71f;
                    }
                    else
                    {
                        if (cround == 1)
                        {
                            destination = Dest.East;
                            _velocity.X = this._initialVelocity;
                            _velocity.Y = 0;
                            rotation = 0f;
                        }
                        else if (cround == -1)
                        {
                            destination = Dest.West;
                            _velocity.X = -this._initialVelocity;
                            _velocity.Y = 0;
                            rotation = 3.14f;
                        }
                    }

                    // Skret zostal zakonczony, wiec zmieniamy wartosc flagi.
                    rotatefirstAiLeft = false;
                    rotatefirstAiRight = false;
                }
                // Jezeli nie skonczylismy skrecac
                else
                {
                    // Zmieniamy predkosc (a co za tym idzie - przesuniecie) zgodnie
                    // z zmienna obrotu i predkoscia skretu.
                    _velocity.X = (float)Math.Cos(rotation) * tangentialVelocity;
                    _velocity.Y = (float)Math.Sin(rotation) * tangentialVelocity;
                }
            }

            // Tworzymy zmienna tablicowa, gdzie przechowamy kolor planszy bezposrednio pod samochodem
            Color[] c = new Color[1];
            // Pobieramy kolor planszy pod samochodem.
            background.GetData(0, new Rectangle((int)this._position.X + 400, (int)this._position.Y, 1, 1), c, 0, c.Length);
            // Zmienne referencyjne - kolor bialy i czarny (ktory na planszy jest niemal bialy i nie do konca czarny).
            // Kolory te wystepuja tylko w miejscu, gdzie konczy sie okrazenie.
            Color black = new Color(43, 42, 41);
            Color white = new Color(254, 254, 254);
            // Jesli kolor pod samochodem to ktorys z kolorow referencyjnych...
            if (c[0] == black || c[0] == white)
            {
                // ...ustawiamy flage zmiany okrazenia.
                lapChange = true;
            }
            // W przeciwnym wypadku...
            else
            {
                // ...sprawdzamy, czy flaga zmiany okrazenia jest podniesiona oraz czy od ostatniej zmiany
                // uplynelo 5 sekund (poniewaz pole, gdzie moze sie dokonac zmiana jest szerokie i w czasie
                // normalnego przejazdu mozna by wielokrotnie przestawic numer aktualnego okrazenia).
                if (lapChange && (DateTime.Now.Subtract(lastLapChange) > TimeSpan.FromSeconds(5)))
                {
                    // Jesli zmieniamy okrazenie - zapisujemy czas zmiany
                    lastLapChange = DateTime.Now;
                    // Oraz zwiekszamy licznik
                    _lap++;
                }
                // Po zmianach zdejmujemy flage zmiany okrazenia.
                lapChange = false;
            }

            // Zmieniamy polozenie auta zgodnie z jego aktualna predkoscia.
            this._position += this._velocity;
        }

        // Funkcja pobierajaca kolor danego punktu na planszy.
        // Parametry - wspolzedne przesuniecia planszy, pozycja, z ktorej ma zostac pobrany kolor, tekstura planszy.
        private Color[] getBackgroundColor(int x, int y, Vector2 position, Texture2D background)
        {
            // Tworzymy prostokat 1x1, o odpowiednich wspolrzednych, z ktorego zostanie pobrana wartosc koloru.
            Rectangle rect = new Rectangle((int)position.X + x, (int)position.Y + y, 1, 1);
            // Tworzymy tablice, do ktorej zapiszemy kolor.
            Color[] c = new Color[1];
            // Jezeli prostokat z ktorego pobieramy kolor znajduje sie poza plansza...
            if (rect.X > background.Width || rect.X < 0 || rect.Y > background.Height || rect.Y < 0)
            {
                // ...zapisujemy kolor przezroczysty, bedacy oznaka, ze cos poszlo zle.
                c[0] = Color.Transparent;
            }
            // W innym wypadku...
            else
            {
                // ...probujemy pobrac kolor z danego miejsca.
                try
                {
                    // Jesli sie uda - zapisujemy wartosc do przygotowanej tablicy.
                    background.GetData(0, rect, c, 0, c.Length);
                }
                catch (Exception ex)
                {
                    // W przypadku wystapienia bledu - zapisujemy przezroczystosc.
                    c[0] = Color.Transparent;
                }
            }
            // Zwracamy otrzymany kolor.
            return c;
        }


    }
}
