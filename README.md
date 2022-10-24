# PolygonEditor
Aplikacja zawiera trzy tryby

- Tryb dodawania wielokąta
- Tryb Edycji
- Tryb krawędzi prostopadłych

Aktualnie używany tryb rozpoznajemy z komunikatu na dole lewego panelu aplikacji. Po starcie jesteśmy domyślnie w trybie dodawania wielokąta.
### Tryb dodawania wielokąta ###
Jest to tryb blokujący wszelkie operacje na wielokątach oprócz dodawania wierzchołków i łączących je krawędzi do aktualnie konstruowanego wielokąta. Robimy to naciskając prawym przyciskiem myszy na ekran, w punkcie naciśnięcia pojawi się wierzchołek. Żeby zamknąć wielokąt i przejść w tryb edycji trzeba nacisnąć na pierwszy dodany wierzchołek aktualnego wielokąta (A).

Kombinacja PPM + Lewy Shift doda krawędź, której długość będzie ustalona. Będzie ona kolorowana na czerwono z oznaczeniem "L".

### Tryb Edycji ###
Przechodzimy w ten tryb po utworzeniu wielokąta. Akcje dostępne z poziomu widoku wielokątów:
- PPM na dowolny wierzchołek: wyświetla się menu z poziomu którego możemy usunąć ten wierzchołek.
- PPM na krawędź: wyświetla się menu z poziomu którego możemy dodać wierzchołek na środku tej krawędzi.
- Drag wierzchołka za pomocą LPM - przesuwa wierzchołek po ekranie z zachowaniem relacji innych wierzchołków.
- Drag wielokąta za pomocą LPM - należy złapać wielokąt w okolicach jego środka (gdy kursor myszy zmieni się na odpowiedni), umożliwia przenoszenie wielokąta bez zmiany długości krawędzi.
- Drag krawędzi za pomocą LPM - przesuwa krawędź do wybranego miejsca
- Przejście w tryb dodawania wielokąta za pomocą PPM na widoku - rozpoczyna tworzenie nowego wielokąta.

Lewy panel zawiera trzy przyciski udostępniające funkcje globalne dla wszystkich wielokątów:

- Dodaj relacje ORTH - Otwiera tryb krawędzi prostopadłych.
- Bresenham - Ustawia tryb rysowania za pomocą algorytmu Bresenhama
- Library - Ustawia tryb rysowania za pomocą algorytmu bibliotecznego

Przycisk 'Usuń wielokąt' usuwa ostatni wielokąt z jakiego korzystaliśmy (za pomocą PPM lub LPM na wierzchołek lub krawędź). Lewy panel zawiera również trzy karty wyświetlające dane tego właśnie wielokąta:
- Wierzchołki - lista jego wierzchołków wraz z położeniem
- Krawędzie - lista krawędzi z nazwami i długością
- Relacje - odpowiednio nazwa krawędzi, rodzaj relacji ('FIX_L' oznacza relację zachowania długości, 'ORTH_nr' oznacza relację prostopadłości, gdzie 'nr' identyfikuje krawędzie będące ze sobą w relacji) oraz przycisk 'usuń' usuwający relację w wielokącie.

### Tryb krawędzi prostopadłych ###
Osiągalny z poziomu odpowiedniego przycisku w lewym panelu. W tym trybie musimy wybrać dwie krawędzie (LPM) na które zostanie nałożona relacja prostopadłości. Inne funkcje są zablokowane. Jedynym sposobem opuszczenia trybu jest wybór krawędzi do relacji, przechodzimy wtedy do trybu Edycji.
