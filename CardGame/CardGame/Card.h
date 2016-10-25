#pragma once
#include "CardDeck.h"
class Card
{
public:
    Card(CardDeck::CardValue value, CardDeck::CardColor color);
    ~Card();

    bool operator<(const Card& card);
    bool operator>(const Card& card);
    bool operator==(const Card& card);

private:
    CardDeck::CardValue _value;
    CardDeck::CardColor _color;
};

