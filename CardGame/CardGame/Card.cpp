#include "stdafx.h"
#include "Card.h"


Card::Card(CardDeck::CardValue value, CardDeck::CardColor color)
{
}

Card::~Card()
{
}

bool Card::operator<(const Card& card)
{
    return false;
}

bool Card::operator>(const Card & card)
{
    return false;
}

bool Card::operator==(const Card & card)
{
    return false;
}


