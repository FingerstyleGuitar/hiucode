#pragma once
#include <vector>

class Card;

class CardDeck
{
public:
    CardDeck();
    ~CardDeck();

    vector<shared_ptr<Card>> GetCard(int count);
    int GetCardCount();

public:
    enum CardValue
    {
        ACE = 1,
        TWO,
        THREE,
        FOUR,
        FIVE,
        SIX,
        SEVEN,
        EIGHT,
        NINE,
        TEN,
        JACK,
        QUEEN,
        KING
    };

    enum CardColor
    {
        SPADE = 1,
        HEART,
        CLUB,
        DIAMOND
    };

private:
    typedef vector<shared_ptr<Card>> CardList;
    typedef CardList::iterator ItCard;

private:
    CardList _card_deck;
};

