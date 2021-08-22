using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace NotNumberPad
{
	public class Flash : IEnumerable<Button> {

        private List<Button> buttons = new List<Button>();
        public IEnumerator<Button> GetEnumerator()
        { return buttons.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator()
        { return GetEnumerator(); }

		public Flash(IEnumerable<Button> buttons)
        {
            this.buttons = new List<Button> (buttons);
        }
        public Button this[int i]
        {
            get { return buttons[i]; }
        }

        public int[] GetNumbers()
        {
            return buttons.Select(x => x.value.Value).ToArray();
        }
        public int GetValue(int[] priorities)
        {
            int val = 0;
            int[] nums = GetNumbers();
            foreach (int num in priorities)
                if (nums.Contains(num))
                    val = val * 10 + num;
            return val;
        }
        public override string ToString()
        {
            var buttonCols = buttons.Select(x => x.color);
            string rtn = "";
            for (int i = 0; i < 4; i++)
                if (buttonCols.Contains((ButtonColor)i))
                    rtn += ((ButtonColor)i).ToString()[0].ToString();
            return rtn;
        }
    }

}
